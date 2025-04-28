using System.Linq.Expressions;
using CoffeePeek.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace CoffeePeek.Data;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public TEntity First(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.First(predicate);
    }

    public TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.FirstOrDefault(predicate);
    }

    public TEntity? FirstOrDefault()
    {
        return _dbSet.FirstOrDefault();
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public IQueryable<TEntity> GetAll()
    {
        return _dbSet.AsNoTracking();
    }

    public async Task<List<TEntity>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.AsNoTracking().Where(predicate);
    }

    public bool Any(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Any(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public TEntity Find(params object[] keys)
    {
        return _dbSet.Find(keys)!;
    }

    public async Task<TEntity?> FindAsync(params object[] keys)
    {
        return await _dbSet.FindAsync(keys);
    }

    public void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        _dbSet.AddRange(entities);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public void DeleteRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public IOrderedQueryable<TEntity> OrderBy<K>(Expression<Func<TEntity, K>> predicate)
    {
        return _dbSet.OrderBy(predicate);
    }

    public IQueryable<IGrouping<K, TEntity>> GroupBy<K>(Expression<Func<TEntity, K>> predicate)
    {
        return _dbSet.GroupBy(predicate);
    }
}