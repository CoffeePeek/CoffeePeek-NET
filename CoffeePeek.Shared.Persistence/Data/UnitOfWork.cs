using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wolverine;

namespace CoffeePeek.Shared.Persistence.Data;

public class UnitOfWork<TDbContext>(TDbContext context, IMessageBus bus) : IUnitOfWork
    where TDbContext : DbContext
{
    private IDbContextTransaction? _transaction;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var entities = context.ChangeTracker
            .Entries<IEntity>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .Select(x => x.Entity)
            .ToList();
        
        var result = await context.SaveChangesAsync(ct);

        foreach (var entity in entities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                await bus.PublishAsync(domainEvent); 
            }

            entity.ClearDomainEvents();
        }
        
        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null) return;
        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null) throw new InvalidOperationException("No transaction");

        try
        {
            await context.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
        }
        catch
        {
            await _transaction.RollbackAsync(ct);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}