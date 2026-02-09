using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;

namespace CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsNoTracking(Guid id, CancellationToken ct);
    
    Task<(IReadOnlyList<Review> reviews, int totalCount)> GetByCoffeeShopId(
        Guid coffeeShopId, 
        int page = 1, 
        int pageSize = 10, 
        CancellationToken ct = default);

    Task<(IReadOnlyList<Review> reviews, decimal avgRating, int totalCount)>
        GetReviewsWithStatsByCoffeeShopId(Guid coffeeShopId, CancellationToken ct);
    
    Task<Dictionary<Guid, (decimal AverageRating, int Count)>> GetReviewStatsByShopIds(List<Guid> shopIds, CancellationToken ct);
}