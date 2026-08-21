using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MegaHairPro.Web.Models
{
    public class AgendamentoServico
    {
        public int AgendamentoId { get; set; }
        public int ServicoId { get; set; }

        /// <summary>Profissional que realizou este serviço (pode diferir do agendamento principal).</summary>
        public int? ProfissionalId { get; set; }

        /// <summary>Indica se o serviço já foi realizado no atendimento.</summary>
        public bool Realizado { get; set; }

        public Agendamento Agendamento { get; set; }
        public Servico Servico { get; set; }
        public Profissional? Profissional { get; set; }
    }
}
