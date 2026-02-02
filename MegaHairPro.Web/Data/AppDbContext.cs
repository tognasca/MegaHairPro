using Microsoft.EntityFrameworkCore;
using MegaHairPro.Web.Models;

namespace MegaHairPro.Web.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Despesas> Despesas => Set<Despesas>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Estoque> Estoques => Set<Estoque>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<MetodoAplicacao> MetodosAplicacao => Set<MetodoAplicacao>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<SimulacaoVenda> Simulacao => Set<SimulacaoVenda>();

    public DbSet<Comissao> Comissoes { get; set; }
}