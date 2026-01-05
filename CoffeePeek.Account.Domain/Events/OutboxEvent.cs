using CoffeePeek.Shared.Infrastructure.Outbox;

namespace CoffeePeek.Account.Domain.Events;

public class OutboxEvent : IOutboxEventEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; }
    public DateTime? ProcessedAt { get; set; } = DateTime.UtcNow;
}