namespace CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

public interface IReviewRepository
{
    void Add(Review review);
    Task<bool> AnyAsync(Guid shopId, Guid reviewId, CancellationToken ct);
    Task<Review?> GetById(Guid reviewId, CancellationToken ct);
    Task<Review[]> GetByUserId(Guid eventUserId, CancellationToken ct);
}