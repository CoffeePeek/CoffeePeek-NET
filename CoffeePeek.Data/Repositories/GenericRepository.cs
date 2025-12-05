using System.Linq.Expressions;
using CoffeePeek.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Data.Repositories;

public sealed class GenericRepository<TEntity, TDbContext>(TDbContext context) : IGenericRepository<TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    private readonly TDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id!], cancellationToken);
    }

    public async Task<TEntity[]> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToArrayAsync(cancellationToken);
    }
    
    public async Task<TEntity[]> GetAllAsNoTrackingAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToArrayAsync(cancellationToken);
    }

    public async Task<TEntity[]> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToArrayAsync(cancellationToken);
    }

    public async Task<TEntity[]> FindAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToArrayAsync(cancellationToken);
    }
    
    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }
    
    public async Task<TEntity?> FirstOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.UpdateRange(entities);
    }

    public void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.RemoveRange(entities);
    }

    public IQueryable<TEntity> Query()
    {
        return _dbSet;
    }

    public IQueryable<TEntity> QueryAsNoTracking()
    {
        return _dbSet.AsNoTracking();
    }
}