using System.Collections.Generic;

namespace MegaHairPro.Web.Models;

public class MetodoAplicacao
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public decimal ValorPorGrama { get; set; }
    public decimal ValorTotalConfeccao { get; set; }
    public bool Status { get; set; }
}
