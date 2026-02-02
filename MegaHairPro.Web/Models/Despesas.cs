using System;

namespace MegaHairPro.Web.Models;
public class Despesas
{
    public int Id { get; set; }
    public string Descricao { get; set; } = "";
    public decimal Valor { get; set; }

    public bool Ratear { get; set; } = false;

    public string Categoria { get; set; } = ""; // ✅ NOVO

    public DateTime DataCadastro { get; set; } = DateTime.Now;
    public bool Status { get; set; } = true;
}