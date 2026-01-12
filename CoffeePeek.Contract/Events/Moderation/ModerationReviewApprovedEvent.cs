using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Contract.Events.Moderation;

public record ModerationReviewApprovedEvent(ModerationReviewDto Review) : IOutboxEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}