using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;
using Review = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class AdminStatsQueryRepository(ShopsDbContext dbContext) : IAdminStatsQueryRepository
{
    public async Task<(int TotalCoffeeShops, int NewToday, int TotalReviews, int NewReviewsToday)> GetStatsAsync(
        CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        var totalCoffeeShops = await dbContext.Shops.CountAsync(ct);
        var newCoffeeShopsToday = await dbContext.Shops.CountAsync(s => s.CreatedAtUtc >= today, ct);
        var totalReviews = await dbContext.Set<Review>().CountAsync(ct);
        var newReviewsToday = await dbContext.Set<Review>().CountAsync(r => r.CreatedAtUtc >= today, ct);

        return (totalCoffeeShops, newCoffeeShopsToday, totalReviews, newReviewsToday);
    }
}
