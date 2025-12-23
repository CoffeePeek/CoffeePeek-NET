using CoffeePeek.Moderation.Domain.Entities;

namespace CoffeePeek.Moderation.Domain.Events;

public record ModerationShopApprovedDomainEvent(ModerationShop Shop) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public ModerationShop Shop { get; } = Shop;
}