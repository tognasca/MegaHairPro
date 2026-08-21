using System;

namespace MegaHairPro.Web.Models
{
    public class Venda
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteNome { get; set; }
        public int ProdutoId { get; set; }
        public string? ProdutoNome { get; set; }
        public decimal QuantidadeGramas { get; set; }
        public decimal? PrecoVenda { get; set; }
        public decimal? CustoTotal { get; set; }
        public decimal? LucroTotal { get; set; }
        public decimal ValorComissaoVendedora { get; set; }
        public decimal ValorComissaoCabeleireira { get; set; }
        public DateTime DataVenda { get; set; } = DateTime.Now;
    }
}