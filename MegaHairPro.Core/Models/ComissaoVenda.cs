using System;

namespace MegaHairPro.Web.Models
{
    public class ComissaoVenda
    {
        public int Id { get; set; }
        public int VendaId { get; set; }

        public decimal Percentual { get; set; }
        public decimal Valor { get; set; }

        public DateTime Data { get; set; }
    }

}
