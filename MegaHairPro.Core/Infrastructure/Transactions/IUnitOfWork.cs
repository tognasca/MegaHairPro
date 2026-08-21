
using System;
namespace MegaHairPro.Web.Infrastructure.Transactions;
public interface IUnitOfWork : IDisposable
{
    void Begin();
    void Commit();
    void Rollback();
}
