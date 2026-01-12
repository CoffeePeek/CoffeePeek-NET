using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Contract.Events.Moderation;

public record ModerationShopApprovedEvent(Guid UserId, ShopDto Shop) : IOutboxEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}