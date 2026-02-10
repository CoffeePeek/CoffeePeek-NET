namespace CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsNoTracking(Guid id, CancellationToken ct);
    void Update(Review review);
    Task<(bool exists, Guid? reviewId)> ExistsForCurrentUser(Guid shopId, Guid userId, CancellationToken ct);
    Task<Review?> GetById(Guid id, CancellationToken ct);
    
    Task<(IReadOnlyList<Review> reviews, int totalCount)> GetByCoffeeShopId(
        Guid coffeeShopId, 
        int page = 1, 
        int pageSize = 10, 
        CancellationToken ct = default);
    
    Task<(decimal averageRating, int count)> GetReviewStatsByCoffeeShopId(Guid coffeeShopId, CancellationToken ct);
    
    Task<Dictionary<Guid, (decimal averageRating, int count)>> GetReviewStatsByShopIds(List<Guid> shopIds, CancellationToken ct);
}