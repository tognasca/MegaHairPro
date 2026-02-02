using System;

namespace MegaHairPro.Web.Models
{    public class Comissao
    {
        public int Id { get; set; }

        public int VendaId { get; set; }
        public string Profissional { get; set; } = "";

        public decimal Valor { get; set; }

        public bool Pago { get; set; } = false;

        public DateTime DataGeracao { get; set; } = DateTime.Now;
        public DateTime? DataPagamento { get; set; }
    }
}