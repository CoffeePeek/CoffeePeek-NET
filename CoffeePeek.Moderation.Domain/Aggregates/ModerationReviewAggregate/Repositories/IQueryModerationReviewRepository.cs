namespace CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;

public interface IQueryModerationReviewRepository
{
    Task<ModerationReview[]> GetAll(CancellationToken cancellationToken);
}