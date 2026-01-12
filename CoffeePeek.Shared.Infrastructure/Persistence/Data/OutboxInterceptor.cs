using System.Text.Json;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoffeePeek.Shared.Infrastructure.Persistence.Data;

public class OutboxInterceptor<TOutboxEvent> : SaveChangesInterceptor 
    where TOutboxEvent : OutboxEvent, new()
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken ct = default)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, ct);

        var now = DateTime.UtcNow;

        var entitiesWithEvents = context.ChangeTracker
            .Entries<IEntity>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .ToList();

        foreach (var entry in entitiesWithEvents)
        {
            var eventsForOutbox = entry.Entity.DomainEvents
                .OfType<IOutboxEvent>() 
                .ToList();

            if (eventsForOutbox.Count == 0) continue;
            var outboxMessages = eventsForOutbox.Select(domainEvent => new TOutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = domainEvent.GetType().Name,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                CreatedAt = now,
                Processed = false
            });

            context.Set<TOutboxEvent>().AddRange(outboxMessages);
            
            foreach (var processedEvent in eventsForOutbox)
            {
                entry.Entity.RemoveDomainEvent(processedEvent); 
            }
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }
}