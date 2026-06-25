using CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryCommunityCityFollowRepository(ShopsDbContext dbContext) : IQueryCommunityCityFollowRepository
{
    public async Task<IReadOnlyList<Guid>> GetFollowedCityIdsAsync(Guid userId, CancellationToken ct = default) =>
        await dbContext.CommunityCityFollows
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.CreatedAtUtc)
            .Select(f => f.CityId)
            .ToListAsync(ct);

    public Task<bool> IsFollowingCityAsync(Guid userId, Guid cityId, CancellationToken ct = default) =>
        dbContext.CommunityCityFollows
            .AsNoTracking()
            .AnyAsync(f => f.UserId == userId && f.CityId == cityId, ct);
}
