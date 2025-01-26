using Core;
using Microsoft.EntityFrameworkCore;
namespace EntityModels.Interfaces;

public interface IUnitOfWork<TContext> where TContext : IDbContext, new()
{
    IGenericRepository<TEntity> GetGenericRepository<TEntity>() where TEntity : class;
    void Dispose();
    void SaveChanges();
    void RevertChanges();
    void DetachAllEntities();
    void Restart();
    IDbContext ReturnContext();

}
