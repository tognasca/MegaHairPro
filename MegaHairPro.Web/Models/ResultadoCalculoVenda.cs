namespace MegaHairPro.Web.Models
{
    public class ResultadoCalculoVenda
    {
        public decimal CustoMedioPorGrama { get; set; }
        public decimal ValorCabelo { get; set; }
        public decimal ValorMetodo { get; set; }
        public decimal ValorEnvio { get; set; }

        // 🔹 Juros do parcelamento (preenchido depois)
        public decimal Juros { get; set; }

        // 🔹 Total final (à vista ou parcelado)
        public decimal ValorTotal { get; set; }
    }
}
