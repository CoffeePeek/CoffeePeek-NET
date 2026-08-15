namespace CoffeePeek.Account.Application.Features.Admin.Stats;

public interface IAdminStatsClient
{
    Task<AdminPlatformStatsSnapshot> GetPlatformStatsAsync(CancellationToken cancellationToken = default);
}

public sealed record AdminPlatformStatsSnapshot(
    int TotalCoffeeShops,
    int TotalReviews,
    int NewCoffeeShopsToday,
    int NewReviewsToday,
    int PendingModerationShops,
    int PendingModerationReviews,
    int ImportPending,
    int ImportPublished,
    int ImportRejected,
    int ImportSkipped,
    bool ShopsAvailable,
    bool ModerationAvailable);
