using System.Linq.Expressions;

namespace CoffeePeek.Data.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity[]> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity[]> GetAllAsNoTrackingAsync(CancellationToken cancellationToken = default);
    Task<TEntity[]> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity[]> FindAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
    
    IQueryable<TEntity> Query();
    IQueryable<TEntity> QueryAsNoTracking();
}