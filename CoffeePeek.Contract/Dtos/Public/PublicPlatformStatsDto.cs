namespace CoffeePeek.Contract.Dtos.Public;

public record PublicPlatformStatsDto(
    int TotalCoffeeShops,
    int TotalReviews,
    int TotalCheckIns,
    decimal AverageRating);
