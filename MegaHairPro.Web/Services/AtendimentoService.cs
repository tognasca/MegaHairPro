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
    public List<AtendimentoServicoDto> Servicos { get; set; } = [];
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

public class AtendimentoService
{
    private readonly AppDbContext _context;

    public AtendimentoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AtendimentoResumoDto>> ListarAgendaHojeAsync()
    {
        var hoje = DateTime.Today;
        var agendamentos = await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Profissional)
            .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
            .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Profissional)
            .Where(a => a.DataHora.Date == hoje && a.Status != StatusAgendamento.Cancelado)
            .OrderBy(a => a.DataHora)
            .ToListAsync();

        return agendamentos.Select(MapResumo).ToList();
    }

    public async Task<List<AtendimentoResumoDto>> BuscarClienteAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return [];

        termo = termo.Trim().ToLower();
        var clientes = await _context.Clientes
            .Where(c => c.Nome.ToLower().Contains(termo) ||
                        (c.Telefone != null && c.Telefone.Contains(termo)) ||
                        (c.WhatsApp != null && c.WhatsApp.Contains(termo)))
            .Take(20)
            .ToListAsync();

        var clienteIds = clientes.Select(c => c.Id).ToList();
        var agendamentos = await _context.Agendamentos
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
        var ag = await CarregarAgendamentoAsync(agendamentoId);
        return ag == null ? null : MapResumo(ag);
    }

    /// <summary>Check-in walk-in: cliente sem horário marcado.</summary>
    public async Task<AtendimentoResumoDto> CriarWalkInAsync(int clienteId, int profissionalId, int[] servicosIds)
    {
        if (!servicosIds.Any())
            throw new InvalidOperationException("Selecione ao menos um serviço.");

        var servicos = await _context.Servicos
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
            Status = StatusAgendamento.Agendado,
            PercentualSinal = 0,
            ValorSinal = 0,
            ValorRestante = valorTotal,
            SinalPago = true,
            DataPagamentoSinal = agora
        };

        _context.Agendamentos.Add(agendamento);
        await _context.SaveChangesAsync();

        foreach (var servicoId in servicosIds)
        {
            _context.AgendamentoServicos.Add(new AgendamentoServico
            {
                AgendamentoId = agendamento.Id,
                ServicoId = servicoId,
                ProfissionalId = profissionalId
            });
        }

        await _context.SaveChangesAsync();

        var carregado = await CarregarAgendamentoAsync(agendamento.Id);
        return MapResumo(carregado!);
    }

    public async Task AtualizarServicosAsync(int agendamentoId, List<AtendimentoServicoDto> servicos)
    {
        var ag = await _context.Agendamentos
            .Include(a => a.AgendamentosServicos)
            .FirstOrDefaultAsync(a => a.Id == agendamentoId);

        if (ag == null)
            throw new InvalidOperationException("Atendimento não encontrado.");

        if (ag.Status == StatusAgendamento.Concluido)
            throw new InvalidOperationException("Atendimento já concluído.");

        var idsNovos = servicos.Select(s => s.ServicoId).ToHashSet();
        var remover = ag.AgendamentosServicos.Where(s => !idsNovos.Contains(s.ServicoId)).ToList();
        _context.AgendamentoServicos.RemoveRange(remover);

        foreach (var dto in servicos)
        {
            var existente = ag.AgendamentosServicos.FirstOrDefault(s => s.ServicoId == dto.ServicoId);
            if (existente == null)
            {
                _context.AgendamentoServicos.Add(new AgendamentoServico
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

        await RecalcularValoresAsync(ag);
        await _context.SaveChangesAsync();
    }

    public async Task RegistrarSinalAsync(int agendamentoId, string? formaPagamento = null)
    {
        var ag = await _context.Agendamentos.FindAsync(agendamentoId)
            ?? throw new InvalidOperationException("Atendimento não encontrado.");

        if (ag.SinalPago)
            throw new InvalidOperationException("Sinal já registrado.");

        if (ag.ValorSinal <= 0)
            throw new InvalidOperationException("Este atendimento não possui sinal.");

        ag.SinalPago = true;
        ag.DataPagamentoSinal = DateTime.Now;
        ag.Status = StatusAgendamento.Confirmado;

        _context.ContasFinanceiras.Add(CriarContaReceber(
            ag, ag.ValorSinal, $"Sinal — Agendamento #{ag.Id}", formaPagamento));

        await _context.SaveChangesAsync();
    }

    public async Task RegistrarRestanteAsync(int agendamentoId, string? formaPagamento = null)
    {
        var ag = await _context.Agendamentos.FindAsync(agendamentoId)
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
            _context.ContasFinanceiras.Add(CriarContaReceber(
                ag, ag.ValorRestante, $"Restante — Agendamento #{ag.Id}", formaPagamento));
        }

        await _context.SaveChangesAsync();
    }

    public async Task RegistrarPagamentoTotalAsync(int agendamentoId, string? formaPagamento = null)
    {
        var ag = await _context.Agendamentos
            .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
            .FirstOrDefaultAsync(a => a.Id == agendamentoId)
            ?? throw new InvalidOperationException("Atendimento não encontrado.");

        if (ag.Status == StatusAgendamento.Concluido)
            throw new InvalidOperationException("Atendimento já concluído.");

        var valorTotal = ag.AgendamentosServicos.Sum(s => s.Servico.ValorFinal);

        if (ag.SinalPago && ag.ValorSinal > 0)
        {
            await RegistrarRestanteAsync(agendamentoId, formaPagamento);
            return;
        }

        ag.SinalPago = ag.ValorSinal > 0;
        ag.RestantePago = true;
        ag.DataConclusao = DateTime.Now;
        ag.Status = StatusAgendamento.Concluido;

        _context.ContasFinanceiras.Add(CriarContaReceber(
            ag, valorTotal, $"Atendimento — Agendamento #{ag.Id}", formaPagamento));

        await _context.SaveChangesAsync();
    }

    public async Task<List<Cliente>> ListarClientesAsync(string? filtro = null)
    {
        var query = _context.Clientes.Where(c => c.Status).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            filtro = filtro.Trim().ToLower();
            query = query.Where(c =>
                c.Nome.ToLower().Contains(filtro) ||
                (c.Telefone != null && c.Telefone.Contains(filtro)) ||
                (c.WhatsApp != null && c.WhatsApp.Contains(filtro)));
        }

        return await query.OrderBy(c => c.Nome).Take(50).ToListAsync();
    }

    public async Task<List<Servico>> ListarServicosAsync()
        => await _context.Servicos.OrderBy(s => s.Categoria).ThenBy(s => s.Nome).ToListAsync();

    public async Task<List<Profissional>> ListarProfissionaisAsync()
        => await _context.Profissionais.OrderBy(p => p.Nome).ToListAsync();

    private async Task RecalcularValoresAsync(Agendamento ag)
    {
        var servicos = await _context.AgendamentoServicos
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

    private async Task<Agendamento?> CarregarAgendamentoAsync(int id)
        => await _context.Agendamentos
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
