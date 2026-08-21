using System;

namespace MegaHairPro.Web.Models;

public enum TipoContaFinanceira
{
    Pagar = 1,
    Receber = 2
}

public enum OrigemContaFinanceira
{
    Venda = 1,
    Despesa = 2,
    Manual = 3,
    Agendamento = 4  // ← novo
}

public enum StatusContaFinanceira
{
    Aberta = 1,
    Paga = 2,
    Cancelada = 3
}

public class ContaFinanceira
{
    public int Id { get; set; }

    public TipoContaFinanceira Tipo { get; set; }
    public OrigemContaFinanceira Origem { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }

    public DateTime DataCompetencia { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }

    public string? FormaPagamento { get; set; }

    public StatusContaFinanceira Status { get; set; } = StatusContaFinanceira.Aberta;

    // FK — Venda
    public int? VendaId { get; set; }
    public Venda? Venda { get; set; }

    // FK — Despesa
    public int? DespesaId { get; set; }
    public Despesas? Despesa { get; set; }

    // FK — Agendamento ← novo
    public int? AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }
}