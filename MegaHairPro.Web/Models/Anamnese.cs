using System;

namespace MegaHairPro.Web.Models;

public class Anamnese
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public string Alergias { get; set; }
    public string HistoricoClinico { get; set; }
    public string ProcedimentosAnteriores { get; set; }
    public string Restricoes { get; set; }
    public string Observacoes { get; set; }

    public string AssinaturaDigital { get; set; }
    public bool Ativa { get; set; } = true;
}

