using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;
using Review = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class AdminStatsQueryRepository(ShopsDbContext dbContext) : IAdminStatsQueryRepository
{
    public async Task<AdminShopsStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var totalCoffeeShops = await dbContext.Shops.CountAsync(cancellationToken);
        var newCoffeeShopsToday = await dbContext.Shops.CountAsync(s => s.CreatedAtUtc >= today, cancellationToken);
        var totalReviews = await dbContext.Set<Review>().CountAsync(cancellationToken);
        var newReviewsToday = await dbContext.Set<Review>().CountAsync(r => r.CreatedAtUtc >= today, cancellationToken);

        return new AdminShopsStats(totalCoffeeShops, newCoffeeShopsToday, totalReviews, newReviewsToday);
    }
}
