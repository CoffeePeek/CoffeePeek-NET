using CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class CommunityCityFollowRepository(ShopsDbContext dbContext) : ICommunityCityFollowRepository
{
    public void Add(CommunityCityFollow follow) => dbContext.CommunityCityFollows.Add(follow);

    public void Remove(CommunityCityFollow follow) => dbContext.CommunityCityFollows.Remove(follow);

    public Task<CommunityCityFollow?> GetAsync(Guid userId, Guid cityId, CancellationToken ct = default) =>
        dbContext.CommunityCityFollows
            .FirstOrDefaultAsync(f => f.UserId == userId && f.CityId == cityId, ct);
}
