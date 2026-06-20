namespace CoffeePeek.Contract.Dtos.Admin;

public record AdminOverviewStatsDto(
    int TotalUsers,
    int UsersRegisteredToday,
    int TotalCoffeeShops,
    int TotalReviews,
    int PendingModerationShops,
    int PendingModerationReviews,
    int NewCoffeeShopsToday,
    int NewReviewsToday);
