using System.Collections.Generic;

namespace MegaHairPro.Web.Models;

public class MetodoAplicacao
{
    public int Id { get; set; }

    public string Nome { get; set; }

    public string Descricao { get; set; }

    public bool Status { get; set; }
}
