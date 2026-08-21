using System;

namespace MegaHairPro.Web.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Telefone { get; set; }
        public string WhatsApp { get; set; }

        public string Email { get; set; }

        public string CPF { get; set; }

        public DateTime? DataNascimento { get; set; }

        public string Endereco { get; set; }

        public string Observacoes { get; set; }

        public bool Status { get; set; }
    }
}
