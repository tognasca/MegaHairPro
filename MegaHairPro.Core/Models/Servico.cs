using System.ComponentModel.DataAnnotations;

namespace MegaHairPro.Web.Models
{
    public enum CategoriaServico
    {
        Cabelo = 1,
        Unha = 2,
        Tratamento = 3,
        MegaHair = 4,
        Maquiagem = 5
    }

    public class Servico
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public string? Descricao { get; set; }

        [Required]
        public decimal Valor { get; set; }

        public decimal? ValorPromocional { get; set; }

        public int DuracaoMinutos { get; set; }


        public CategoriaServico Categoria { get; set; }

        public string? ImagemUrl { get; set; }

        public decimal ValorFinal =>
            ValorPromocional.HasValue && ValorPromocional.Value > 0
                ? ValorPromocional.Value
                : Valor;
    }
}