using System;
using System.ComponentModel.DataAnnotations;

namespace MegaHairPro.Web.Models;

public class ConfiguracaoBooking
{
    [Key]
    [MaxLength(80)]
    public string Chave { get; set; } = "";

    [MaxLength(500)]
    public string Valor { get; set; } = "";

    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}
