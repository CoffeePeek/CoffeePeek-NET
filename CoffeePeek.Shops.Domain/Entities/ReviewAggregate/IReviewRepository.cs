namespace CoffeePeek.Shops.Domain.Entities.ReviewAggregate;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsNoTracking(Guid id, CancellationToken ct);
    void Update(Review review);
    Task<(bool, Guid?)> ExistsForCurrentUser(Guid shopId, Guid userId, CancellationToken ct);
    Task<Review?> GetById(Guid id, CancellationToken ct);
}