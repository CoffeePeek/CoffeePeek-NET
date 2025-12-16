using CoffeePeek.Shared.Infrastructure.Outbox;

namespace CoffeePeek.AuthService.Entities;

public class OutboxEvent : IOutboxEventEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;
    public DateTime? ProcessedAt { get; set; }
}