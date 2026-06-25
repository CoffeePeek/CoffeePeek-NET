using CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class CommunityUserFollowRepository(ShopsDbContext dbContext) : ICommunityUserFollowRepository
{
    public void Add(CommunityUserFollow follow) => dbContext.CommunityUserFollows.Add(follow);

    public void Remove(CommunityUserFollow follow) => dbContext.CommunityUserFollows.Remove(follow);

    public Task<CommunityUserFollow?> GetAsync(Guid followerId, Guid followingUserId, CancellationToken ct = default) =>
        dbContext.CommunityUserFollows
            .FirstOrDefaultAsync(
                f => f.FollowerId == followerId && f.FollowingUserId == followingUserId,
                ct);
}
