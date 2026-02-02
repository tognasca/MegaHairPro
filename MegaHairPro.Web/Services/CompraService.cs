
using MegaHairPro.Web.Data;
using MegaHairPro.Web.Infrastructure.Transactions;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MegaHairPro.Web.Services.Implementations;
public class CompraService : ICompraService
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _uow;

    public CompraService(AppDbContext context, IUnitOfWork uow)
    {
        _context = context;
        _uow = uow;
    }

    public void Criar(Compra compra)
    {
        _uow.Begin();
        try
        {
            _context.Compras.Add(compra);

            var estoque = _context.Estoques
                .FirstOrDefault(e => e.ProdutoId == compra.ProdutoId);

            if (estoque == null)
            {
                estoque = new Estoque
                {
                    ProdutoId = compra.ProdutoId,
                    QuantidadeGramas = compra.QuantidadeGramas ?? 0
                };
                _context.Estoques.Add(estoque);
            }
            else
            {
                estoque.QuantidadeGramas += compra.QuantidadeGramas ?? 0;
            }

            _context.SaveChanges();
            _uow.Commit();
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public void Editar(Compra compra)
    {
        _uow.Begin();
        try
        {
            var original = _context.Compras.AsNoTracking()
                .First(c => c.Id == compra.Id);

            var diferenca = (compra.QuantidadeGramas ?? 0) -
                            (original.QuantidadeGramas ?? 0);

            var estoque = _context.Estoques
                .First(e => e.ProdutoId == compra.ProdutoId);

            estoque.QuantidadeGramas += diferenca;

            _context.Compras.Update(compra);
            _context.SaveChanges();

            _uow.Commit();
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public void Excluir(int id)
    {
        _uow.Begin();
        try
        {
            var compra = _context.Compras.Find(id);
            var estoque = _context.Estoques
                .First(e => e.ProdutoId == compra.ProdutoId);

            estoque.QuantidadeGramas -= compra.QuantidadeGramas ?? 0;

            _context.Compras.Remove(compra);
            _context.SaveChanges();

            _uow.Commit();
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
