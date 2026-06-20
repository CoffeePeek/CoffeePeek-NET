namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public interface IAdminStatsQueryRepository
{
    Task<(int TotalCoffeeShops, int NewToday, int TotalReviews, int NewReviewsToday)> GetStatsAsync(
        CancellationToken ct = default);
}
