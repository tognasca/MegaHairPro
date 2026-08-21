using System.ComponentModel.DataAnnotations;

namespace MegaHairPro.Web.Models
{
    public class Fornecedor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome e obrigatorio")]
        [StringLength(150, ErrorMessage = "O nome pode ter no maximo 150 caracteres")]
        public string Nome { get; set; } = "";

        [StringLength(100)]
        public string Contato { get; set; } = "";

        [StringLength(18)]
        [RegularExpression(@"^(\d{11,14})?$", ErrorMessage = "CNPJ deve conter 11 a 14 numeros")]
        public string CNPJ { get; set; } = "";

        [StringLength(20)]
        public string? InscricaoEstadual { get; set; }

        [StringLength(100)]
        public string? RazaoSocial { get; set; }

        [StringLength(100)]
        public string? NomeFantasia { get; set; }

        public string? Telefone { get; set; }

        public string? Email { get; set; }

        [StringLength(150)]
        public string? Logradouro { get; set; }

        [StringLength(20)]
        public string? Numero { get; set; }

        [StringLength(50)]
        public string? Complemento { get; set; }

        [StringLength(100)]
        public string? Bairro { get; set; }

        [StringLength(50)]
        public string? Cidade { get; set; }

        [StringLength(2)]
        public string? UF { get; set; }

        [StringLength(10)]
        [RegularExpression(@"^(\d{8})?$", ErrorMessage = "CEP deve conter 8 numeros")]
        public string? CEP { get; set; }

        [StringLength(50)]
        public string? Banco { get; set; }

        [StringLength(20)]
        public string? Agencia { get; set; }

        [StringLength(20)]
        public string? Conta { get; set; }

        [StringLength(20)]
        public string? TipoConta { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public bool Status { get; set; }
    }
}
