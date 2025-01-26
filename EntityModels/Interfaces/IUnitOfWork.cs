using Core;
namespace EntityModels.Interfaces;

public interface IUnitOfWork<TContext> where TContext : IDbContext, new()
{
    IGenericRepository<TEntity> GetGenericRepository<TEntity>() where TEntity : class;
    void Dispose();
    void SaveChanges();
    Task SaveChangesAsync();
    void RevertChanges();
    void DetachAllEntities();
    void Restart();
    IDbContext ReturnContext();

}
