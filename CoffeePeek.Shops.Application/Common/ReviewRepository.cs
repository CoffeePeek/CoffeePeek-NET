using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using Microsoft.EntityFrameworkCore;
using Review = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Application.Common;

public class ReviewRepository(IGenericRepository<Review> reviewRepository) : IReviewRepository
{
    public async Task<Review?> GetByIdAsNoTracking(Guid reviewId, CancellationToken ct)
    {
        return await reviewRepository.FirstOrDefaultAsNoTrackingAsync(x => x.Id == reviewId, ct);
    }

    public async Task<(IReadOnlyList<Review> reviews, int totalCount)> GetByCoffeeShopId(
        Guid coffeeShopId, 
        int page = 1, 
        int pageSize = 10, 
        CancellationToken ct = default)
    {
        var query = reviewRepository
            .QueryAsNoTracking()
            .Where(r => r.CoffeeShopId == coffeeShopId && !r.IsSoftDelete)
            .OrderByDescending(r => r.CreatedAtUtc);

        var totalCount = await query.CountAsync(ct);
        
        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (reviews, totalCount);
    }

    public async Task<(IReadOnlyList<Review> reviews, decimal avgRating, int totalCount)> 
        GetReviewsWithStatsByCoffeeShopId(Guid coffeeShopId, CancellationToken ct)
    {
        var baseQuery = reviewRepository
            .QueryAsNoTracking()
            .Where(r => r.CoffeeShopId == coffeeShopId && !r.IsSoftDelete);

        var stats = await baseQuery
            .GroupBy(r => r.CoffeeShopId)
            .Select(g => new
            {
                Avg = g.Average(r => r.Rating.AverageRating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        var reviews = await baseQuery
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(ct);

        return (reviews, stats?.Avg ?? 0m, stats?.Count ?? 0);
    }

    public async Task<Dictionary<Guid, (decimal AverageRating, int Count)>> GetReviewStatsByShopIds(
        IReadOnlyList<Guid> shopIds, 
        CancellationToken ct)
    {
        if (shopIds.Count == 0) 
            return new Dictionary<Guid, (decimal, int)>();

        var statsList = await reviewRepository
            .QueryAsNoTracking()
            .Where(r => shopIds.Contains(r.CoffeeShopId) && !r.IsSoftDelete)
            .Select(r => new
            {
                r.CoffeeShopId,
                RatingSum = r.Rating.AverageRating
            })
            .GroupBy(r => r.CoffeeShopId)
            .Select(g => new
            {
                ShopId = g.Key,
                Avg = g.Average(x => x.RatingSum),
                Count = g.Count()
            })
            .ToListAsync(ct);

        return statsList.ToDictionary(
            x => x.ShopId, 
            x => (averageRating: x.Avg, count: x.Count));
    }

    public IQueryable<Guid> GetQueryableGroupByIdAndSortByRating(decimal minRating)
    {
        return reviewRepository
            .QueryAsNoTracking()
            .Where(r => !r.IsSoftDelete)
            .GroupBy(r => r.CoffeeShopId)
            .Where(g => g.Average(r => r.Rating.AverageRating) >= minRating)
            .Select(g => g.Key);
    }
}