using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;

public record ModerationReviewApprovedDomainEvent(ModerationReview Review) : IOutboxEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}