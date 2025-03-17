using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace EntityModels.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class
{
    TEntity Delete(object id);
    void Delete(TEntity entity);
    void DeleteRange(IEnumerable<TEntity> entities);
    IEnumerable<TEntity> Get(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

    Task<IEnumerable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);
    IQueryable<TEntity> GetAsQueryable(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

    IQueryable<TEntity> GetAsQueryableWhereIf(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

    TEntity GetByID(object id);
    TEntity Insert(TEntity entity);
    Task<TEntity> InsertAsync(TEntity entity);
    void InsertRange(IEnumerable<TEntity> entities);
    TEntity Update(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    void UpdateWithRelatedEntities(TEntity entity);
    object SetObjectStateToDetached(Object obj);
    object SetObjectStateToAdded(object obj);
    bool Exists(Expression<Func<TEntity, bool>> filter = null);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? filter = null);
    void DeleteRange(Expression<Func<TEntity, bool>> filter = null);
}
