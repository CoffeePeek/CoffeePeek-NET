using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Contract.Events.Shops;

public record ShopCreatedEvent : IOutboxEvent
{
    public Guid ShopId { get; init; }
    public Guid ModerationId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
