namespace CoffeePeek.Contract.Dtos.Admin;

public record AdminServiceStatsDto(
    int TotalCoffeeShops = 0,
    int TotalReviews = 0,
    int NewCoffeeShopsToday = 0,
    int NewReviewsToday = 0,
    int PendingModerationShops = 0,
    int PendingModerationReviews = 0);
