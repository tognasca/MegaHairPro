using System;

namespace MegaHairPro.Web.Models
{
    public class CompraViewModel
    {
        public int Id { get; set; }

        public int ProdutoId { get; set; }
        public int FornecedorId { get; set; }

        public string ProdutoNome { get; set; }
        public string FornecedorNome { get; set; }

        public decimal QuantidadeGramas { get; set; }
        public decimal ValorTotal { get; set; }

        public DateTime DataCompra { get; set; }
        public bool Ativo { get; set; }
    }

}
