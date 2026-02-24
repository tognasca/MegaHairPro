using System.Linq;

namespace MegaHairPro.Web.Models
{
    public static class AgendamentoDuracaoHelper
    {
        public static int CalcularDuracaoTotal(Agendamento agendamento)
        {
            return agendamento.AgendamentosServicos
                .Sum(s => s.Servico.DuracaoMinutos);
        }
    }
}
