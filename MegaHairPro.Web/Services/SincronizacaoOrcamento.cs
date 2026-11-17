using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MegaHairPro.Web.Services;

public sealed class SincronizacaoOrcamento(
    IServiceScopeFactory scopes, IHttpClientFactory clientes,
    IConfiguration config, ILogger<SincronizacaoOrcamento> logger) : BackgroundService
{
    private sealed record Item(string Id, string Nome, string Grupo, string Calculo, decimal Valor, int Quantidade = 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var chave = config["OrcamentoSimulador:Chave"];
        if (string.IsNullOrWhiteSpace(chave)
            || !Uri.TryCreate(config["OrcamentoSimulador:ApiUrl"], UriKind.Absolute, out var endereco)
            || endereco.Scheme != Uri.UriSchemeHttps)
            return;

        var endpoint = new Uri(endereco, "/api/orcamento/sincronizar");
        using var intervalo = new PeriodicTimer(TimeSpan.FromMinutes(2));

        do
        {
            try { await EnviarAsync(endpoint, chave, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogWarning(ex, "Falha ao sincronizar tabela do simulador"); }
        }
        while (await intervalo.WaitForNextTickAsync(stoppingToken));
    }

    private async Task EnviarAsync(Uri endpoint, string chave, CancellationToken ct)
    {
        using var escopo = scopes.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
        var custos = escopo.ServiceProvider.GetRequiredService<ICustoMedioService>();
        var booking = escopo.ServiceProvider.GetRequiredService<BookingConfigService>();

        var orcamento = await booking.LerOrcamentoSimuladorAsync();
        var itens = new List<Item>();
        foreach (var origem in CatalogoSimulador.Origens)
        {
            var codigo = CatalogoSimulador.Codigo(config, origem.Chave);
            var ids = await db.Produtos.Where(p => p.Codigo == codigo && p.Status && p.TipoProduto == TipoProduto.Cabelo)
                .Select(p => p.Id).Take(2).ToListAsync(ct);
            if (ids.Count != 1) continue;
            var custo = custos.CalculaCustoCabelo(ids[0], 1m, orcamento.PercentualLucro).SingleOrDefault();
            if (custo is null || Convert.ToDecimal(custo["custo"]) <= 0) continue;
            itens.Add(new(origem.Chave, origem.Origem, "origem", "grama",
                Convert.ToDecimal(custo["custo"]) + Convert.ToDecimal(custo["lucro"])));
        }

        var metodos = await db.MetodosAplicacao.Where(m => m.Status && m.Nome != null && m.ValorPorGrama >= 0)
            .OrderBy(m => m.Nome).ThenBy(m => m.Id).ToListAsync(ct);
        itens.AddRange(metodos.Where(m => !string.IsNullOrWhiteSpace(m.Nome))
            .GroupBy(m => m.Nome!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(grupo => grupo.Count() == 1)
            .Select(grupo => new Item(grupo.Key, grupo.Key, "metodo", "grama", grupo.Single().ValorPorGrama)));

        var despesas = await db.Despesas.Where(d => d.Ratear).Select(d => d.Valor).ToListAsync(ct);
        itens.Add(new("rateio", "Despesas rateadas", "incluso", "fixo", despesas.Sum(valor => valor / 50)));
        itens.Add(new("coloracao", "Coloração", "consulta", "fixo", 0));

        using var req = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = JsonContent.Create(new { mostrarAntes = orcamento.MostrarAntes, itens })
        };
        req.Headers.Add("X-Orcamento-Chave", chave);
        using var resposta = await clientes.CreateClient("SincronizacaoOrcamento").SendAsync(req, ct);
        resposta.EnsureSuccessStatusCode();
    }
}
