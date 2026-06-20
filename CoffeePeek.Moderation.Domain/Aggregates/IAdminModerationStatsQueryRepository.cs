namespace CoffeePeek.Moderation.Domain.Aggregates;

public interface IAdminModerationStatsQueryRepository
{
    Task<(int PendingShops, int PendingReviews)> GetStatsAsync(CancellationToken ct = default);
}
