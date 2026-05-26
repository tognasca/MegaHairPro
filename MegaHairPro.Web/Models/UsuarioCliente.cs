using System;
using System.ComponentModel.DataAnnotations;

namespace MegaHairPro.Web.Models;

public class UsuarioCliente
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [MaxLength(500)]
    public string? Senha { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;
}
