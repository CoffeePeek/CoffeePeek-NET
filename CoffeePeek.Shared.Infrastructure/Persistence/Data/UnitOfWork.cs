using System.Text.Json;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoffeePeek.Shared.Infrastructure.Persistence.Data;

public class UnitOfWork<TDbContext, TOutboxEvent>(TDbContext context, IMediator mediator) : IUnitOfWork
    where TDbContext : DbContext
    where TOutboxEvent : OutboxEvent, new()
{
    private IDbContextTransaction? _transaction;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await ProcessDomainEventsThroughMediator(cancellationToken);

        InsertOutboxMessages();

        return await context.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessDomainEventsThroughMediator(CancellationToken ct)
    {
        while (true)
        {
            var domainEntities = context.ChangeTracker
                .Entries<IEntity>()
                .Where(x => x.Entity.DomainEvents.Count != 0)
                .ToList();

            if (domainEntities.Count == 0) break;

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            domainEntities.ForEach(x => x.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await mediator.Publish(domainEvent, ct);
            }
        }
    }

    private void InsertOutboxMessages()
    {
        var entitiesWithEvents = context.ChangeTracker
            .Entries<IEntity>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .ToList();

        foreach (var entry in entitiesWithEvents)
        {
            var outboxMessages = entry.Entity.DomainEvents.Select(domainEvent => new TOutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = domainEvent.GetType().Name,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                CreatedAt = DateTime.UtcNow,
                Processed = false
            });

            context.Set<TOutboxEvent>().AddRange(outboxMessages);

            entry.Entity.ClearDomainEvents();
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null) return;
        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Transaction has not been started.");

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
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