namespace CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;

public interface IModerationReviewRepository
{
    Task<ModerationReview[]> GetAllPending();
    Task<ModerationReview?> GetById(Guid id);
    Task<ModerationReview?> GetByShopId(Guid shopId);
    void Add(ModerationReview review);
    void Update(ModerationReview review);
}