using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;

namespace CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsNoTracking(Guid id, CancellationToken ct);
    void Update(Review review);
    Task<(bool exists, Guid? reviewId)> ExistsForCurrentUser(Guid shopId, Guid userId, CancellationToken ct);
    Task<Review?> GetById(Guid id, CancellationToken ct);
    
    /// <summary>
    /// Gets reviews for a coffee shop with pagination support
    /// </summary>
    Task<(IReadOnlyList<Review> reviews, int totalCount)> GetByCoffeeShopIdAsync(
        Guid coffeeShopId, 
        int page = 1, 
        int pageSize = 10, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Gets average rating and review count for a specific coffee shop
    /// </summary>
    Task<(decimal averageRating, int count)> GetReviewStatsByCoffeeShopIdAsync(Guid coffeeShopId, CancellationToken ct);
    
    /// <summary>
    /// Gets average ratings and review counts for multiple coffee shops (batch operation)
    /// </summary>
    Task<Dictionary<Guid, (decimal averageRating, int count)>> GetReviewStatsByShopIdsAsync(List<Guid> shopIds, CancellationToken ct);
}