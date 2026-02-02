namespace MegaHairPro.Web.Models;

public class SimulacaoParcelamentoResult
{
    public int Parcelas { get; set; }
    public decimal ValorParcela { get; set; }

    // 🔹 Juros total cobrado no parcelamento
    public decimal JurosTotal { get; set; }

    // 🔹 Valor final da venda
    public decimal Total { get; set; }
}
