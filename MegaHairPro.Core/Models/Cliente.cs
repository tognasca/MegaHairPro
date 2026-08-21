using System;
using System.ComponentModel.DataAnnotations;

namespace MegaHairPro.Web.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome e obrigatorio")]
        [StringLength(150)]
        public string? Nome { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [StringLength(20)]
        public string? WhatsApp { get; set; }

        [EmailAddress(ErrorMessage = "E-mail invalido")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(14)]
        public string? CPF { get; set; }

        public DateTime? DataNascimento { get; set; }

        [StringLength(250)]
        public string? Endereco { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public bool Status { get; set; }
    }
}
