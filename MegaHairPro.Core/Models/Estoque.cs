namespace MegaHairPro.Web.Models;
public class Estoque
{
    public int Id { get; set; }

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; }

    // Para cabelo
    public decimal QuantidadeGramas { get; set; }

    // Para outros produtos
    public int QuantidadeUnidade { get; set; }

    public decimal CustoMedio { get; set; }
}
