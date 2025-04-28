using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CoffeePeek.Domain.UnitOfWork;

public interface IUnitOfWork<out TContext> : IUnitOfWork where TContext : DbContext
{
    /// <summary>
    /// Gets the db context.
    /// </summary>
    /// <returns>The instance of type <typeparamref name="TContext"/>.</returns>
    TContext DbContext { get; }

    /// <summary>
    /// Saves all changes made in this context to the database with distributed transaction.
    /// </summary>
    /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="unitOfWorks">An optional <see cref="IUnitOfWork"/> array.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default, bool ensureAutoHistory = false,
        params IUnitOfWork[] unitOfWorks);
}

/// <summary>
/// Defines the interface(s) for unit of work.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the specified repository for the <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="hasCustomRepository"><c>True</c> if providing custom repository.</param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>An instance of type inherited from <see cref="IRepository{TEntity}"/> interface.</returns>
    IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : class;

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
    /// <returns>The number of state entries written to the database.</returns>
    int SaveChanges(bool ensureAutoHistory = false);

    /// <summary>
    /// Asynchronously saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default, bool ensureAutoHistory = false);

    /// <summary>
    /// Executes the specified raw SQL command.
    /// </summary>
    /// <param name="sql">The raw SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The number of state entities written to database.</returns>
    int ExecuteSqlCommand(string sql, params object[] parameters);

    /// <summary>
    /// Uses raw SQL queries to fetch the specified <typeparamref name="TEntity"/> data.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="sql">The raw SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the query to execute.</param>
    /// <returns>An <see cref="IQueryable{T}"/> that contains elements that satisfy the condition specified by raw SQL.</returns>
    Task<IQueryable<TEntity>> FromSqlAsync<TEntity>(string sql, CancellationToken cancellationToken = default,
        params object[] parameters) where TEntity : class;

    /// <summary>
    /// Uses TrackGraph API to attach disconnected entities.
    /// </summary>
    /// <param name="rootEntity">Root entity.</param>
    /// <param name="callback">Delegate to convert Object's State properties to Entities entry state.</param>
    void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback);
}