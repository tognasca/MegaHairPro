
using MegaHairPro.Web.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace MegaHairPro.Web.Infrastructure.Transactions;
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public void Begin() => _transaction = _context.Database.BeginTransaction();
    public void Commit() => _transaction?.Commit();
    public void Rollback() => _transaction?.Rollback();
    public void Dispose() => _transaction?.Dispose();
}
