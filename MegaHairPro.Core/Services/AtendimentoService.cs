using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Services;

public class AtendimentoResumoDto
{
    public int AgendamentoId { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = "";
    public string? ClienteTelefone { get; set; }
    public DateTime DataHora { get; set; }
    public DateTime DataFim { get; set; }
    public StatusAgendamento Status { get; set; }
    public bool SinalPago { get; set; }
    public bool RestantePago { get; set; }
    public decimal ValorSinal { get; set; }
    public decimal ValorRestante { get; set; }
    public decimal ValorTotal { get; set; }
    public string ProfissionalPrincipal { get; set; } = "";
    public bool WalkIn { get; set; }
    public int ProfissionalId { get; set; }
    public string? ClienteObservacoes { get; set; }
    public List<AtendimentoServicoDto> Servicos { get; set; } = [];
}

public class ResumoDiaDto
{
    public int Total { get; set; }
    public int Aguardando { get; set; }
    public int Confirmados { get; set; }
    public int Concluidos { get; set; }
    public decimal Faturado { get; set; }
}

public class AnamneseAlertaDto
{
    public bool TemAnamnese { get; set; }
    public string? Alergias { get; set; }
    public string? Restricoes { get; set; }
    public string? Observacoes { get; set; }
    public bool TemAlerta =>
        !string.IsNullOrWhiteSpace(Alergias) || !string.IsNullOrWhiteSpace(Restricoes);
}

public class AtendimentoServicoDto
{
    public int ServicoId { get; set; }
    public string Nome { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal Valor { get; set; }
    public int? ProfissionalId { get; set; }
    public string? ProfissionalNome { get; set; }
    public bool Realizado { get; set; }
}

/// <summary>
/// Uses IDbContextFactory instead of an injected DbContext so this service can safely be
/// registered as a singleton (as it typically is in desktop apps) without different threads
/// or overlapping async calls sharing one DbContext instance. Every public method creates
/// its own short-lived context via CreateDbContextAsync() and disposes it (await using) when done.
/// </summary>
public class AtendimentoService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public AtendimentoService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public Task<List<AtendimentoResumoDto>> ListarAgendaHojeAsync()
        => ListarAgendaPorDataAsync(DateTime.Today);

    public async Task<List<AtendimentoResumoDto>> ListarAgendaPorDataAsync(DateTime data)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var dia = data.Date;
        var agendamentos = await context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Profissional)
            .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
            .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Profissional)
            .Where(a => a.DataHora.Date == dia && a.Status != StatusAgendamento.Cancelado)
            .OrderBy(a => a.DataHora)
            .ToListAsync();

        return agendamentos.Select(MapResumo).ToList();
    }

    public async Task<ResumoDiaDto> ObterResumoDiaAsync(DateTime data)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var dia = data.Date;
        var lista = await context.Agendamentos
            .Where(a => a.DataHora.Date == dia && a.Status != StatusAgendamento.Cancelado)
            .Select(a => a.Status)
            .ToListAsync();

        var faturado = await context.ContasFinanceiras
            .Where(c => c.Tipo == TipoContaFinanceira.Receber
                        && c.Status == StatusContaFinanceira.Paga
                        && c.DataPagamento != null
                        && c.DataPagamento.Value.Date == dia)
            .SumAsync(c => (decimal?)c.Valor) ?? 0;

        return new ResumoDiaDto
        {
            Total = lista.Count,
            Aguardando = lista.Count(s => s == StatusAgendamento.Agendado),
            Confirmados = lista.Count(s => s == StatusAgendamento.Confirmado),
            Concluidos = lista.Count(s => s == StatusAgendamento.Concluido),
            Faturado = faturado
        };
    }

    public async Task<List<AtendimentoResumoDto>> BuscarClienteAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return [];

        await using var context = await _factory.CreateDbContextAsync();

        termo = termo.Trim().ToLower();
        var clientes = await context.Clientes
            .Where(c => (c.Nome != null && c.Nome.ToLower().Contains(termo)) ||
                        (c.Telefone != null && c.Telefone.Contains(termo)) ||
                        (c.WhatsApp != null && c.WhatsApp.Contains(termo)))
            .Take(20)
            .ToListAsync();

        var clienteIds = clientes.Select(c => c.Id).ToList();
        var agendamentos = await context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Profissional)
            .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
            .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Profissional)
            .Where(a => clienteIds.Contains(a.ClienteId) && a.Status != StatusAgendamento.Cancelado)
            .OrderByDescending(a => a.DataHora)
            .Take(30)
            .ToListAsync();

        return agendamentos.Select(MapResumo).ToList();
    }

    public async Task<AtendimentoResumoDto?> ObterAtendimentoAsync(int agendamentoId)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var ag = await CarregarAgendamentoAsync(context, agendamentoId);
        return ag == null ? null : MapResumo(ag);
    }

    /// <summary>Check-in walk-in: cliente sem horário marcado.</summary>
    public async Task<AtendimentoResumoDto> CriarWalkInAsync(int clienteId, int profissionalId, int[] servicosIds)
    {
        if (!servicosIds.Any())
            throw new InvalidOperationException("Selecione ao menos um serviço.");

        await using var context = await _factory.CreateDbContextAsync();

        var servicos = await context.Servicos
            .Where(s => servicosIds.Contains(s.Id))
            .ToListAsync();

        var valorTotal = servicos.Sum(s => s.ValorFinal);
        var duracao = servicos.Sum(s => s.DuracaoMinutos);
        var agora = DateTime.Now;

        var agendamento = new Agendamento
        {
            ClienteId = clienteId,
            ProfissionalId = profissionalId,
            ServicoId = servicosIds[0],
            DataHora = agora,
            DataFim = agora.AddMinutes(duracao > 0 ? duracao : 60),
            Status = StatusAgendamento.Confirmado,
            PercentualSinal = 0,
            ValorSinal = 0,
            ValorRestante = valorTotal,
            SinalPago = true,
            DataPagamentoSinal = agora
        };

        context.Agendamentos.Add(agendamento);
        await context.SaveChangesAsync();

        foreach (var servicoId in servicosIds)
        {
            context.AgendamentoServicos.Add(new AgendamentoServico
            {
                AgendamentoId = agendamento.Id,
                ServicoId = servicoId,
                ProfissionalId = profissionalId
            });
        }

        await context.SaveChangesAsync();

        var carregado = await CarregarAgendamentoAsync(context, agendamento.Id);
        return MapResumo(carregado!);
    }

    public async Task AtualizarServicosAsync(int agendamentoId, List<AtendimentoServicoDto> servicos)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var ag = await context.Agendamentos
            .Include(a => a.AgendamentosServicos)
            .FirstOrDefaultAsync(a => a.Id == agendamentoId);

        if (ag == null)
            throw new InvalidOperationException("Atendimento não encontrado.");

        if (ag.Status == StatusAgendamento.Concluido)
            throw new InvalidOperationException("Atendimento já concluído.");

        var idsNovos = servicos.Select(s => s.ServicoId).ToHashSet();
        var remover = ag.AgendamentosServicos.Where(s => !idsNovos.Contains(s.ServicoId)).ToList();
        context.AgendamentoServicos.RemoveRange(remover);

        foreach (var dto in servicos)
        {
            var existente = ag.AgendamentosServicos.FirstOrDefault(s => s.ServicoId == dto.ServicoId);
            if (existente == null)
            {
                context.AgendamentoServicos.Add(new AgendamentoServico
                {
                    AgendamentoId = agendamentoId,
                    ServicoId = dto.ServicoId,
                    ProfissionalId = dto.ProfissionalId,
                    Realizado = dto.Realizado
                });
            }
            else
            {
                existente.ProfissionalId = dto.ProfissionalId;
                existente.Realizado = dto.Realizado;
            }
        }

        if (servicos.Any())
            ag.ServicoId = servicos[0].ServicoId;

        await RecalcularValoresAsync(context, ag);
        await context.SaveChangesAsync();
    }

    public async Task RegistrarSinalAsync(int agendamentoId, string? formaPagamento = null)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var ag = await context.Agendamentos.FindAsync(agendamentoId)
            ?? throw new InvalidOperationException("Atendimento não encontrado.");

        if (ag.SinalPago)
            throw new InvalidOperationException("Sinal já registrado.");

        if (ag.ValorSinal <= 0)
            throw new InvalidOperationException("Este atendimento não possui sinal.");

        ag.SinalPago = true;
        ag.DataPagamentoSinal = DateTime.Now;
        ag.Status = StatusAgendamento.Confirmado;

        context.ContasFinanceiras.Add(CriarContaReceber(
            ag, ag.ValorSinal, $"Sinal — Agendamento #{ag.Id}", formaPagamento));

        await context.SaveChangesAsync();
    }

    public async Task RegistrarRestanteAsync(int agendamentoId, string? formaPagamento = null)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var ag = await context.Agendamentos.FindAsync(agendamentoId)
            ?? throw new InvalidOperationException("Atendimento não encontrado.");

        if (ag.Status == StatusAgendamento.Concluido)
            throw new InvalidOperationException("Atendimento já concluído.");

        if (!ag.SinalPago && ag.ValorSinal > 0)
            throw new InvalidOperationException("Registre o sinal antes de receber o restante.");

        if (ag.RestantePago)
            throw new InvalidOperationException("Restante já registrado.");

        ag.RestantePago = true;
        ag.DataConclusao = DateTime.Now;
        ag.Status = StatusAgendamento.Concluido;

        if (ag.ValorRestante > 0)
        {
            context.ContasFinanceiras.Add(CriarContaReceber(
                ag, ag.ValorRestante, $"Restante — Agendamento #{ag.Id}", formaPagamento));
        }

        await context.SaveChangesAsync();
    }

    public async Task RegistrarPagamentoTotalAsync(int agendamentoId, string? formaPagamento = null)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var ag = await context.Agendamentos
            .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
            .FirstOrDefaultAsync(a => a.Id == agendamentoId)
            ?? throw new InvalidOperationException("Atendimento não encontrado.");

        if (ag.Status == StatusAgendamento.Concluido)
            throw new InvalidOperationException("Atendimento já concluído.");

        var valorTotal = ag.AgendamentosServicos.Sum(s => s.Servico.ValorFinal);

        if (ag.SinalPago && ag.ValorSinal > 0)
        {
            // Reaproveita a mesma lógica/contexto local em vez de chamar o método público
            // (que abriria um novo DbContext) para manter esta operação atômica.
            if (!ag.RestantePago)
            {
                ag.RestantePago = true;
                ag.DataConclusao = DateTime.Now;
                ag.Status = StatusAgendamento.Concluido;

                if (ag.ValorRestante > 0)
                {
                    context.ContasFinanceiras.Add(CriarContaReceber(
                        ag, ag.ValorRestante, $"Restante — Agendamento #{ag.Id}", formaPagamento));
                }

                await context.SaveChangesAsync();
            }

            return;
        }

        ag.SinalPago = ag.ValorSinal > 0;
        ag.RestantePago = true;
        ag.DataConclusao = DateTime.Now;
        ag.Status = StatusAgendamento.Concluido;

        context.ContasFinanceiras.Add(CriarContaReceber(
            ag, valorTotal, $"Atendimento — Agendamento #{ag.Id}", formaPagamento));

        await context.SaveChangesAsync();
    }

    public async Task<List<Cliente>> ListarClientesAsync(string? filtro = null)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var query = context.Clientes.Where(c => c.Status).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            filtro = filtro.Trim().ToLower();
            query = query.Where(c =>
                (c.Nome != null && c.Nome.ToLower().Contains(filtro)) ||
                (c.Telefone != null && c.Telefone.Contains(filtro)) ||
                (c.WhatsApp != null && c.WhatsApp.Contains(filtro)));
        }

        return await query.OrderBy(c => c.Nome).Take(50).ToListAsync();
    }

    public async Task<List<Servico>> ListarServicosAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.Servicos
            .AsNoTracking()
            .OrderBy(s => s.Categoria)
            .ThenBy(s => s.Nome)
            .ToListAsync();
    }

    public async Task<List<Profissional>> ListarProfissionaisAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.Profissionais
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task MarcarChegadaAsync(int agendamentoId)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var ag = await context.Agendamentos.FindAsync(agendamentoId)
            ?? throw new InvalidOperationException("Atendimento não encontrado.");

        if (ag.Status == StatusAgendamento.Concluido)
            throw new InvalidOperationException("Atendimento já concluído.");
        if (ag.Status == StatusAgendamento.Cancelado)
            throw new InvalidOperationException("Atendimento cancelado.");

        ag.Status = StatusAgendamento.Confirmado;
        await context.SaveChangesAsync();
    }

    public async Task CancelarAsync(int agendamentoId)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var ag = await context.Agendamentos.FindAsync(agendamentoId)
            ?? throw new InvalidOperationException("Atendimento não encontrado.");

        if (ag.Status == StatusAgendamento.Concluido)
            throw new InvalidOperationException("Não é possível cancelar um atendimento já concluído.");

        ag.Status = StatusAgendamento.Cancelado;
        ag.Cancelado = true;
        await context.SaveChangesAsync();
    }

    public async Task<Cliente> CriarClienteRapidoAsync(string nome, string? telefone, string? whatsapp, string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new InvalidOperationException("Informe o nome da cliente.");

        await using var context = await _factory.CreateDbContextAsync();

        var cliente = new Cliente
        {
            Nome = nome.Trim(),
            Telefone = telefone?.Trim() ?? "",
            WhatsApp = whatsapp?.Trim() ?? telefone?.Trim() ?? "",
            Email = "",
            CPF = "",
            Endereco = "",
            Observacoes = observacoes?.Trim() ?? "",
            Status = true
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }

    public async Task<AnamneseAlertaDto> ObterAnamneseAsync(int clienteId)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var ana = await context.Anamneses
            .Where(a => a.ClienteId == clienteId && a.Ativa)
            .OrderByDescending(a => a.DataCadastro)
            .FirstOrDefaultAsync();

        if (ana == null)
            return new AnamneseAlertaDto { TemAnamnese = false };

        return new AnamneseAlertaDto
        {
            TemAnamnese = true,
            Alergias = ana.Alergias,
            Restricoes = ana.Restricoes,
            Observacoes = ana.Observacoes
        };
    }

    private static async Task RecalcularValoresAsync(AppDbContext context, Agendamento ag)
    {
        var servicos = await context.AgendamentoServicos
            .Include(s => s.Servico)
            .Where(s => s.AgendamentoId == ag.Id)
            .ToListAsync();

        var valorTotal = servicos.Sum(s => s.Servico.ValorFinal);
        var duracao = servicos.Sum(s => s.Servico.DuracaoMinutos);

        if (ag.PercentualSinal > 0 && !ag.SinalPago)
        {
            ag.ValorSinal = Math.Round(valorTotal * (ag.PercentualSinal / 100m), 2);
            ag.ValorRestante = Math.Round(valorTotal - ag.ValorSinal, 2);
        }
        else if (ag.SinalPago && ag.ValorSinal > 0)
        {
            ag.ValorRestante = Math.Round(valorTotal - ag.ValorSinal, 2);
        }
        else
        {
            ag.ValorSinal = 0;
            ag.ValorRestante = valorTotal;
        }

        ag.DataFim = ag.DataHora.AddMinutes(duracao > 0 ? duracao : 60);
    }

    private static ContaFinanceira CriarContaReceber(
        Agendamento ag, decimal valor, string descricao, string? formaPagamento)
    {
        return new ContaFinanceira
        {
            Tipo = TipoContaFinanceira.Receber,
            Origem = OrigemContaFinanceira.Agendamento,
            Descricao = descricao,
            Valor = valor,
            DataCompetencia = DateTime.Now,
            DataVencimento = DateTime.Now,
            Status = StatusContaFinanceira.Paga,
            DataPagamento = DateTime.Now,
            FormaPagamento = formaPagamento,
            AgendamentoId = ag.Id
        };
    }

    private static async Task<Agendamento?> CarregarAgendamentoAsync(AppDbContext context, int id)
        => await context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Profissional)
            .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
            .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Profissional)
            .FirstOrDefaultAsync(a => a.Id == id);

    private static AtendimentoResumoDto MapResumo(Agendamento a)
    {
        var valorTotal = a.AgendamentosServicos?.Sum(s => s.Servico?.ValorFinal ?? 0) ?? 0;
        return new AtendimentoResumoDto
        {
            AgendamentoId = a.Id,
            ClienteId = a.ClienteId,
            ClienteNome = a.Cliente?.Nome ?? "",
            ClienteTelefone = a.Cliente?.Telefone ?? a.Cliente?.WhatsApp,
            DataHora = a.DataHora,
            DataFim = a.DataFim,
            Status = a.Status,
            SinalPago = a.SinalPago,
            RestantePago = a.RestantePago,
            ValorSinal = a.ValorSinal,
            ValorRestante = a.ValorRestante > 0 ? a.ValorRestante : valorTotal - (a.SinalPago ? a.ValorSinal : 0),
            ValorTotal = valorTotal,
            ProfissionalPrincipal = a.Profissional?.Nome ?? "",
            ProfissionalId = a.ProfissionalId,
            ClienteObservacoes = a.Cliente?.Observacoes,
            WalkIn = a.PercentualSinal == 0 && a.SinalPago && a.ValorSinal == 0,
            Servicos = a.AgendamentosServicos?.Select(s => new AtendimentoServicoDto
            {
                ServicoId = s.ServicoId,
                Nome = s.Servico?.Nome ?? "",
                Categoria = s.Servico?.Categoria.ToString() ?? "",
                Valor = s.Servico?.ValorFinal ?? 0,
                ProfissionalId = s.ProfissionalId ?? a.ProfissionalId,
                ProfissionalNome = s.Profissional?.Nome ?? a.Profissional?.Nome,
                Realizado = s.Realizado
            }).ToList() ?? []
        };
    }
}