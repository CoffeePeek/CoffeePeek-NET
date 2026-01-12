using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Events;

public record ModerationShopApprovedEvent(ModerationShop Shop) : IOutboxEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public ModerationShop Shop { get; } = Shop;
}