using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Infrastructure.Services;

public class UserCheckInRepository(IGenericRepository<CheckIn> repository) : IUserCheckInRepository
{
    public Task<bool> Exists(Guid userId, Guid coffeeShopId, CancellationToken ct = default)
    {
        return repository.AnyAsync(x => x.UserId == userId && x.ShopId == coffeeShopId, ct);
    }

    public Task<List<Guid>> GetVisitedShopIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return repository
            .QueryAsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.ShopId)
            .ToListAsync(ct);
    }

    public Task<int> GetCheckInCountByCoffeeShopIdAsync(Guid coffeeShopId, CancellationToken ct = default)
    {
        return repository
            .QueryAsNoTracking()
            .CountAsync(x => x.ShopId == coffeeShopId, ct);
    }

    public async Task<Dictionary<Guid, int>> GetCheckInCountsByShopIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default)
    {
        var shopIdList = shopIds.ToList();
        
        var counts = await repository
            .QueryAsNoTracking()
            .Where(x => shopIdList.Contains(x.ShopId))
            .GroupBy(x => x.ShopId)
            .Select(g => new { ShopId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return counts.ToDictionary(c => c.ShopId, c => c.Count);
    }
}