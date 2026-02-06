using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Common;

public class ReviewRepository(IGenericRepository<Review> reviewRepository) : IReviewRepository
{
    public async Task<Review?> GetByIdAsNoTracking(Guid reviewId, CancellationToken ct)
    {
        return await reviewRepository.FirstOrDefaultAsNoTrackingAsync(x => x.Id == reviewId, ct);
    }
    
    public async Task<Review?> GetById(Guid reviewId, CancellationToken ct)
    {
        return await reviewRepository.GetByIdAsync(reviewId, ct);
    }

    public void Update(Review review)
    {
        reviewRepository.Update(review);
    }

    public async Task<(bool exists, Guid? reviewId)> ExistsForCurrentUser(Guid shopId, Guid userId, CancellationToken ct)
    {
        var review = await reviewRepository
            .FirstOrDefaultAsNoTrackingAsync(x => x.CoffeeShopId == shopId && x.UserId == userId, ct);
        
        return (review != null, review?.Id);
    }

    public async Task<(IReadOnlyList<Review> reviews, int totalCount)> GetByCoffeeShopIdAsync(
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

    public async Task<(decimal averageRating, int count)> GetReviewStatsByCoffeeShopIdAsync(Guid coffeeShopId, CancellationToken ct)
    {
        var stats = await reviewRepository
            .QueryAsNoTracking()
            .Where(r => r.CoffeeShopId == coffeeShopId && !r.IsSoftDelete)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                AverageRating = g.Average(r => r.Rating.AverageRating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        return stats != null ? (stats.AverageRating, stats.Count) : (0m, 0);
    }

    public async Task<Dictionary<Guid, (decimal averageRating, int count)>> GetReviewStatsByShopIdsAsync(
        IEnumerable<Guid> shopIds, 
        CancellationToken ct)
    {
        var shopIdList = shopIds.ToList();
        
        var stats = await reviewRepository
            .QueryAsNoTracking()
            .Where(r => shopIdList.Contains(r.CoffeeShopId) && !r.IsSoftDelete)
            .GroupBy(r => r.CoffeeShopId)
            .Select(g => new
            {
                CoffeeShopId = g.Key,
                AverageRating = g.Average(r => r.Rating.AverageRating),
                Count = g.Count()
            })
            .ToListAsync(ct);

        return stats.ToDictionary(
            s => s.CoffeeShopId, 
            s => (s.AverageRating, s.Count));
    }
}