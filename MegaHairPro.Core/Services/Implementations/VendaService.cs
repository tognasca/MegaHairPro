
using MegaHairPro.Web.Data;
using MegaHairPro.Web.Domain.Rules;
using MegaHairPro.Web.Infrastructure.Transactions;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services.Interfaces;
using System;

namespace MegaHairPro.Web.Services.Implementations;
public class VendaService : IVendaService
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _uow;

    public VendaService(AppDbContext context, IUnitOfWork uow)
    {
        _context = context;
        _uow = uow;
    }

    public void CriarVenda(SimulacaoVenda simulacao)
    {
        _uow.Begin();
        try
        {
            var produto = _context.Produtos.Find(simulacao.ProdutoId);
            if (produto == null)
            {
                throw new InvalidOperationException("Produto não encontrado.");
            }

            EstoqueRules.Validar(produto.EstoqueGramas, simulacao.QuantidadeGramas);

            var venda = new Venda
            {
                DataVenda = DateTime.Now,
                //ValorTotal = simulacao.PrecoVenda,
                CustoTotal = simulacao.CustoTotal,
                //Lucro = simulacao.Lucro,
                //Margem = simulacao.Margem,
                //Status = "Finalizada",
                ProdutoId = simulacao.ProdutoId,
                QuantidadeGramas = simulacao.QuantidadeGramas
            };

            _context.Vendas.Add(venda);
            produto.EstoqueGramas -= simulacao.QuantidadeGramas;

            _context.SaveChanges();
            _uow.Commit();
        }
        catch (Exception)
        {
            _uow.Rollback();
            throw;
        }
    }
}
