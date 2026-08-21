using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MegaHairPro.Web.Models
{
    public enum StatusAgendamento
    {
        Agendado = 1,
        Confirmado = 2,  // sinal pago
        Concluido = 3,  // atendimento realizado e pago
        Cancelado = 4
    }

    public class Agendamento
    {
        public int Id { get; set; }

        [Required]
        public DateTime DataHora { get; set; }
        public DateTime DataFim { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; }

        public int ServicoId { get; set; }
        public Servico Servico { get; set; }

        public List<AgendamentoServico> AgendamentosServicos { get; set; }

        // ── Status ────────────────────────────────────────────────────────────
        public StatusAgendamento Status { get; set; } = StatusAgendamento.Agendado;

        // Mantido para compatibilidade — use Status == Cancelado
        public bool Cancelado { get; set; } = false;

        // ── Financeiro ────────────────────────────────────────────────────────

        /// <summary>Percentual do sinal cobrado (10 a 50%)</summary>
        public decimal PercentualSinal { get; set; } = 30; // padrão 30%

        /// <summary>Valor do sinal calculado</summary>
        public decimal ValorSinal { get; set; }

        /// <summary>Indica se o sinal foi recebido</summary>
        public bool SinalPago { get; set; } = false;

        public DateTime? DataPagamentoSinal { get; set; }

        /// <summary>Valor restante após o sinal</summary>
        public decimal ValorRestante { get; set; }

        /// <summary>Indica se o valor restante foi recebido (atendimento concluído)</summary>
        public bool RestantePago { get; set; } = false;

        public DateTime? DataConclusao { get; set; }

        // ── Computed ──────────────────────────────────────────────────────────
        [NotMapped]
        public decimal ValorTotal =>
            AgendamentosServicos?.Sum(s => s.Servico?.ValorFinal ?? 0) ?? 0;

        [NotMapped]
        public decimal ValorTotalPago =>
            (SinalPago ? ValorSinal : 0) + (RestantePago ? ValorRestante : 0);
    }
}