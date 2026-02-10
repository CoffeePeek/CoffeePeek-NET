using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
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

    public async Task<(decimal averageRating, int count)> GetReviewStatsByCoffeeShopId(Guid coffeeShopId, CancellationToken ct)
    {
        var stats = await reviewRepository
            .QueryAsNoTracking()
            .Where(r => r.CoffeeShopId == coffeeShopId && !r.IsSoftDelete)
            .Select(g => new
            {
                AverageRating = (g.Rating.Place + g.Rating.Coffee + g.Rating.Service) / 3,
                Count = 1
            })
            .FirstOrDefaultAsync(ct);

        return stats != null ? (stats.AverageRating, stats.Count) : (0m, 0);
    }

    public async Task<Dictionary<Guid, (decimal averageRating, int count)>> GetReviewStatsByShopIds(
        List<Guid> shopIds, 
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
                RatingSum = (r.Rating.Place + r.Rating.Coffee + r.Rating.Service) / 3m
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
}