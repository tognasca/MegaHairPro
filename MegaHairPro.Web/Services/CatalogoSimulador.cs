using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MegaHairPro.Web.Services;

internal static class CatalogoSimulador
{
    internal static readonly (string Chave, string Origem)[] Origens =
    [
        ("brasileiro-regional", "Brasileiro regional"),
        ("brasileiro-do-sul", "Brasileiro do Sul"),
        ("indiano", "Indiano"),
        ("russo", "Russo"),
        ("vietnamita", "Vietnamita")
    ];

    internal sealed record Resultado(IReadOnlyList<Produto> Produtos, int Criados, string? Aviso);

    internal static string Codigo(IConfiguration configuracao, string chave) =>
        configuracao["SimuladorMegaHair:Produtos:" + chave]?.Trim() is { Length: > 0 } codigo
            ? codigo
            : "SIM-" + chave.ToUpperInvariant();

    internal static Resultado CadastrarFaltantes(
        AppDbContext db, IConfiguration configuracao, bool aceitarConflitos)
    {
        var produtos = Origens.Select(origem => new Produto
        {
            Codigo = Codigo(configuracao, origem.Chave),
            Origem = origem.Origem,
            TipoProduto = TipoProduto.Cabelo,
            UnidadeMedida = UnidadeMedida.Grama
        }).ToArray();
        var codigos = produtos.Select(produto => produto.Codigo!).ToArray();

        if (codigos.Distinct(StringComparer.OrdinalIgnoreCase).Count() != codigos.Length)
            return new(produtos, 0, "Configure um código diferente para cada origem.");

        using var transacao = db.Database.BeginTransaction(IsolationLevel.Serializable);
        var existentes = db.Produtos.AsNoTracking()
            .Where(produto => codigos.Contains(produto.Codigo!)).ToList()
            .ToLookup(produto => produto.Codigo);
        var faltantes = new List<Produto>();
        var conflitos = new List<string>();

        foreach (var produto in produtos)
        {
            var encontrados = existentes[produto.Codigo].ToArray();
            if (encontrados.Length == 0)
                faltantes.Add(produto);
            else if (encontrados.Length > 1 || !encontrados[0].Status || encontrados[0].TipoProduto != TipoProduto.Cabelo)
                conflitos.Add(produto.Codigo!);
        }

        var aviso = conflitos.Count == 0 ? null
            : "Revise os produtos inativos ou com código em conflito: " + string.Join(", ", conflitos);
        if (aviso is not null && !aceitarConflitos)
            return new(produtos, 0, aviso);

        foreach (var produto in faltantes)
            produto.Descricao = produto.ObterDescricao();

        if (faltantes.Count > 0)
        {
            db.Produtos.AddRange(faltantes);
            db.SaveChanges();
        }

        transacao.Commit();
        return new(produtos, faltantes.Count, aviso);
    }
}
