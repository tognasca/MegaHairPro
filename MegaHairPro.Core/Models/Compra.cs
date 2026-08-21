using System;
namespace MegaHairPro.Web.Models;
public class Compra
{
    public int Id { get; set; }

    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public int FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }

    public decimal? QuantidadeGramas { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime DataCompra { get; set; }

    public bool Ativo { get; set; } = true; // 👈 NOVO
}
