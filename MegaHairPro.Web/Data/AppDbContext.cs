using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MegaHairPro.Web.Data;
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Despesas> Despesas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Estoque> Estoques { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<MetodoAplicacao> MetodosAplicacao { get; set; }
    public DbSet<Compra> Compras { get; set; }
    public DbSet<Venda> Vendas { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<SimulacaoVenda> Simulacao { get; set; }
    public DbSet<Comissao> Comissoes { get; set; }
}