namespace CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;

public interface IModerationReviewRepository
{
    Task<ModerationReview[]> GetAll(CancellationToken ct = default);
    Task<ModerationReview?> GetById(Guid id, CancellationToken ct = default);
    Task<ModerationReview?> GetByShopId(Guid shopId, CancellationToken ct = default);
    void Add(ModerationReview review);
    void Update(ModerationReview review);
}