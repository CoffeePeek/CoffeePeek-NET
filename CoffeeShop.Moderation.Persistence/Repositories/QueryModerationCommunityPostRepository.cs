using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;
using CoffeePeek.Moderation.Domain.Common.Enums;
using CoffeeShop.Moderation.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Moderation.Persistence.Repositories;

public class QueryModerationCommunityPostRepository(ModerationDbContext dbContext) : IQueryModerationCommunityPostRepository
{
    public Task<ModerationCommunityPost?> GetById(Guid id, CancellationToken ct = default) =>
        dbContext.ModerationCommunityPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(ModerationCommunityPost[] Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        ModerationStatus? status,
        string? search,
        CancellationToken ct = default)
    {
        var query = dbContext.ModerationCommunityPosts.AsNoTracking().AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.ModerationStatus == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.Body.ToLower().Contains(term) ||
                p.UserName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(ct);

        return (items, totalCount);
    }

    public Task<int> CountByUserSinceAsync(Guid userId, DateTime sinceUtc, CancellationToken ct = default) =>
        dbContext.ModerationCommunityPosts
            .AsNoTracking()
            .CountAsync(p => p.UserId == userId && p.CreatedAtUtc >= sinceUtc, ct);
}
