using System.Linq.Expressions;

namespace CoffeePeek.Domain.UnitOfWork;

public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Persists all updates to the data source
    /// </summary>
    void SaveChanges();

    /// <summary>
    /// Persists all updates to the data source async
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get first entity by predicate 
    /// </summary>
    /// <param name="predicate">LINQ predicate</param>
    /// <returns>T entity</returns>
    TEntity First(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Get first entity by predicate 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns>T entity</returns>
    TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Get first entity
    /// </summary>
    /// <returns>T entity</returns>
    TEntity? FirstOrDefault();

    /// <summary>
    /// Get first entity async
    /// </summary>
    /// <returns>T entity</returns>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all queries
    /// </summary>
    /// <returns>IQueryable queries</returns>
    IQueryable<TEntity> GetAll();

    Task<List<TEntity>> GetAllAsync();

    /// <summary>
    /// Find queries by predicate (where logic)
    /// </summary>
    /// <param name="predicate">Search predicate (LINQ)</param>
    /// <returns>IQueryable queries</returns>
    IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Find queries by predicate
    /// </summary>
    /// <param name="predicate">Search predicate (LINQ)</param>
    /// <returns>IQueryable queries</returns>
    bool Any(Expression<Func<TEntity, bool>> predicate);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Find entity by keys
    /// </summary>
    /// <param name="keys">Search key</param>
    /// <returns>T entity</returns>
    TEntity Find(params object[] keys);

    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Add(TEntity entity);

    /// <summary>
    /// Add new entities
    /// </summary>
    /// <param name="entities">Entity collection</param>
    void AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Remove entity from database
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Delete(TEntity entity);

    /// <summary>
    /// Remove entities from database
    /// </summary>
    /// <param name="entity">Entity object</param>
    void DeleteRange(IEnumerable<TEntity> entity);

    /// <summary>
    /// Update entity
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Update(TEntity entity);

    /// <summary>
    /// Order by
    /// </summary>
    IOrderedQueryable<TEntity> OrderBy<K>(Expression<Func<TEntity, K>> predicate);

    /// <summary>
    /// Order by
    /// </summary>
    IQueryable<IGrouping<K, TEntity>> GroupBy<K>(Expression<Func<TEntity, K>> predicate);

    /// <summary>
    /// Remove range of given entities
    /// </summary>
    void RemoveRange(IEnumerable<TEntity> entities);
}