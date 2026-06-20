using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate.Enums;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeeShop.Moderation.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Moderation.Persistence.Repositories;

public class AdminModerationStatsQueryRepository(ModerationDbContext dbContext) : IAdminModerationStatsQueryRepository
{
    public async Task<(int PendingShops, int PendingReviews)> GetStatsAsync(CancellationToken ct = default)
    {
        var pendingShops = await dbContext.ModerationShops
            .CountAsync(s => s.ModerationStatus == ModerationStatus.Pending, ct);

        var pendingReviews = await dbContext.ModerationReviews
            .CountAsync(r => r.ModerationStatus == ModerationStatus.Pending, ct);

        return (pendingShops, pendingReviews);
    }
}
