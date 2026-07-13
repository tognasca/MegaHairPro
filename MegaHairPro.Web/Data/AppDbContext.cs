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
    public DbSet<ContaFinanceira> ContasFinanceiras { get; set; }
    public DbSet<Anamnese> Anamneses { get; set; }


    //**************** SALAO ****************//

    public DbSet<Servico> Servicos { get; set; }
    public DbSet<Profissional> Profissionais { get; set; }
    public DbSet<Agendamento> Agendamentos { get; set; }
    public DbSet<AgendamentoServico> AgendamentoServicos { get; set; }
    public DbSet<ConfiguracaoBooking> ConfiguracoesBooking { get; set; }
    public DbSet<UsuarioCliente> UsuariosClientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AgendamentoServico>()
            .HasKey(x => new { x.AgendamentoId, x.ServicoId });

        modelBuilder.Entity<AgendamentoServico>()
            .HasOne(x => x.Agendamento)
            .WithMany(a => a.AgendamentosServicos)
            .HasForeignKey(x => x.AgendamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AgendamentoServico>()
            .HasOne(x => x.Servico)
            .WithMany()
            .HasForeignKey(x => x.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AgendamentoServico>()
            .HasOne(x => x.Profissional)
            .WithMany()
            .HasForeignKey(x => x.ProfissionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}