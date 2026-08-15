namespace CoffeePeek.Contract.Dtos.Admin;

/// <summary>
/// Platform admin dashboard totals. Missing downstream services return 0
/// and flip the matching <c>*Available</c> flag — the endpoint always succeeds.
/// </summary>
public record AdminOverviewStatsDto(
    int TotalUsers,
    int UsersRegisteredToday,
    int ActiveUsers,
    int BlockedUsers,
    int TotalCoffeeShops,
    int TotalReviews,
    int PendingModerationShops,
    int PendingModerationReviews,
    int NewCoffeeShopsToday,
    int NewReviewsToday,
    AdminImportOverviewStatsDto Import,
    bool ShopsAvailable = true,
    bool ModerationAvailable = true);

public record AdminImportOverviewStatsDto(
    int Pending,
    int Published,
    int Rejected,
    int Skipped,
    int InFeed);
