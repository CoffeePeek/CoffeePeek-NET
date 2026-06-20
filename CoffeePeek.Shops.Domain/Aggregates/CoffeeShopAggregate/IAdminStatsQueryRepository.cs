namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public record AdminShopsStats(
    int TotalCoffeeShops,
    int NewCoffeeShopsToday,
    int TotalReviews,
    int NewReviewsToday);

public interface IAdminStatsQueryRepository
{
    Task<AdminShopsStats> GetStatsAsync(CancellationToken cancellationToken = default);
}
