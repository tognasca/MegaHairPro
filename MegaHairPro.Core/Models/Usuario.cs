using System;

namespace MegaHairPro.Web.Models;
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Login { get; set; } = "";
    public string Senha { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public string Perfil { get; set; } = "Vendedora";
    // Admin | Vendedora | Cabeleireira
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.Now;
}