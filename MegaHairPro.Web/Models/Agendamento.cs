using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace MegaHairPro.Web.Models
{
    public class Agendamento
    {
        public int Id { get; set; }

        [Required]
        public DateTime DataHora { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; }

        public List<AgendamentoServico> AgendamentosServicos { get; set; }
        public int ServicoId { get; set; }
        public Servico Servico { get; set; }
        public decimal ValorTotal => AgendamentosServicos?.Sum(s => s.Servico.Valor) ?? 0;
        public DateTime DataFim { get; set; }
        public bool Cancelado { get; set; } = false;
    }
}
