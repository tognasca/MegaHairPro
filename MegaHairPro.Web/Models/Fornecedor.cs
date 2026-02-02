using System.ComponentModel.DataAnnotations;

namespace MegaHairPro.Web.Models
{
    public class Fornecedor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(150, ErrorMessage = "O nome pode ter no máximo 150 caracteres")]
        public string Nome { get; set; } = "";

        [Required(ErrorMessage = "O contato é obrigatório")]
        [StringLength(100)]
        public string Contato { get; set; } = "";

        [Required(ErrorMessage = "O CNPJ é obrigatório")]
        [RegularExpression(@"\d{14}", ErrorMessage = "CNPJ deve conter 14 números")]
        public string CNPJ { get; set; } = "";

        [StringLength(20)]
        public string InscricaoEstadual { get; set; } = "";

        [StringLength(100)]
        public string RazaoSocial { get; set; } = "";

        [StringLength(100)]
        public string NomeFantasia { get; set; } = "";

        //[Required(ErrorMessage = "O telefone é obrigatório")]
        //[Phone(ErrorMessage = "Telefone inválido")]
        public string Telefone { get; set; } = "";

        //[EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = "";

        // Endereço
        [StringLength(150)]
        public string Logradouro { get; set; } = "";

        [StringLength(20)]
        public string Numero { get; set; } = "";

        [StringLength(50)]
        public string Complemento { get; set; } = "";

        [StringLength(100)]
        public string Bairro { get; set; } = "";

        [StringLength(50)]
        public string Cidade { get; set; } = "";

        [StringLength(2)]
        public string UF { get; set; } = "";

        [StringLength(10)]
        [RegularExpression(@"\d{8}", ErrorMessage = "CEP deve conter 8 números")]
        public string CEP { get; set; } = "";

        // Dados bancários (para pagamentos)
        [StringLength(50)]
        public string Banco { get; set; } = "";

        [StringLength(20)]
        public string Agencia { get; set; } = "";

        [StringLength(20)]
        public string Conta { get; set; } = "";

        [StringLength(20)]
        public string TipoConta { get; set; } = ""; // Corrente ou Poupança

        // Observações adicionais
        [StringLength(500)]
        public string Observacoes { get; set; } = "";


        public bool Status { get; set; }


    }
}
