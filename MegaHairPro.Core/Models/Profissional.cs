using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MegaHairPro.Web.Models
{
    public class Profissional
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public string Especialidade { get; set; }

        public virtual List<Agendamento> Agendamentos { get; set; }
    }
}
