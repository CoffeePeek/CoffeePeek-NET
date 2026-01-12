using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoffeePeek.Shared.Infrastructure.Persistence.Data;

public class UnitOfWork<TDbContext>(TDbContext context, IMediator mediator) : IUnitOfWork
    where TDbContext : DbContext
{
    private IDbContextTransaction? _transaction;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var result = await context.SaveChangesAsync(ct);
        
        var entities = context.ChangeTracker
            .Entries<IEntity>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .ToList();

        foreach (var entry in entities)
        {
            var immediateEvents = entry.Entity.DomainEvents.ToList();
            
            foreach (var domainEvent in immediateEvents)
            {
                await mediator.Publish(domainEvent, ct);
            }

            entry.Entity.ClearDomainEvents();
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