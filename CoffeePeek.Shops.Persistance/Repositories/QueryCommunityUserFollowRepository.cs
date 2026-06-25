using CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryCommunityUserFollowRepository(ShopsDbContext dbContext) : IQueryCommunityUserFollowRepository
{
    public async Task<IReadOnlyList<Guid>> GetFollowingUserIdsAsync(Guid followerId, CancellationToken ct = default) =>
        await dbContext.CommunityUserFollows
            .AsNoTracking()
            .Where(f => f.FollowerId == followerId)
            .OrderBy(f => f.CreatedAtUtc)
            .Select(f => f.FollowingUserId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Guid>> GetFollowerUserIdsAsync(Guid followingUserId, CancellationToken ct = default) =>
        await dbContext.CommunityUserFollows
            .AsNoTracking()
            .Where(f => f.FollowingUserId == followingUserId)
            .OrderBy(f => f.CreatedAtUtc)
            .Select(f => f.FollowerId)
            .ToListAsync(ct);

    public Task<bool> IsFollowingAsync(Guid followerId, Guid followingUserId, CancellationToken ct = default) =>
        dbContext.CommunityUserFollows
            .AsNoTracking()
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingUserId == followingUserId, ct);
}
