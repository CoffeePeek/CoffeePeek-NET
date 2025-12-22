namespace CoffeePeek.Shared.Infrastructure.Outbox;

/// <summary>
/// Interface that outbox event entities must implement.
/// </summary>
public interface IOutboxEventEntity
{
    Guid Id { get; set; }
    string EventType { get; set; }
    string Payload { get; set; }
    DateTime CreatedAt { get; set; }
    bool Processed { get; set; }
    DateTime? ProcessedAt { get; set; }
}

/// <summary>
/// Base entity for outbox events.
/// Should be used as a base class or interface for service-specific outbox entities.
/// </summary>
public abstract class OutboxEvent : IOutboxEventEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;
    public DateTime? ProcessedAt { get; set; }
}

