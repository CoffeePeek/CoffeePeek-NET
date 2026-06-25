namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public record PublicPlatformStats(
    int TotalCoffeeShops,
    int TotalReviews,
    int TotalCheckIns,
    decimal AverageRating);

public interface IPublicStatsQueryRepository
{
    Task<PublicPlatformStats> GetStatsAsync(CancellationToken cancellationToken = default);
}
