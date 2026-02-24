using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MegaHairPro.Web.Models
{
    public class AgendamentoServico
    {
        public int AgendamentoId { get; set; }
        public int ServicoId { get; set; }

        public Agendamento Agendamento { get; set; }
        public Servico Servico { get; set; }
    }
}
