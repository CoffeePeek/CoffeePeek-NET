using CoffeePeek.Account.Domain.Entities.CommunityNotificationAggregate;
using CoffeePeek.Account.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Account.Persistence.Repositories;

public class QueryCommunityNotificationRepository(AccountDbContext dbContext) : IQueryCommunityNotificationRepository
{
    public async Task<(CommunityNotification[] Items, int TotalCount)> GetPageAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = dbContext.CommunityNotifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(ct);

        return (items, totalCount);
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default) =>
        dbContext.CommunityNotifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
}
