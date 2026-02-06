using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Infrastructure.Services;

public class CoffeeShopRepository(IGenericRepository<CoffeeShop> repository) : ICoffeeShopRepository
{
    public Task<bool> Exists(Guid id, CancellationToken ct = default)
    {
        return repository.AnyAsync(x => x.Id == id, ct);
    }

    public async Task<Dictionary<Guid, string>> GetShopNamesByIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default)
    {
        var shopIdList = shopIds.ToList();
        
        var shops = await repository
            .QueryAsNoTracking()
            .Where(s => shopIdList.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);

        return shops.ToDictionary(s => s.Id, s => s.Name);
    }
}