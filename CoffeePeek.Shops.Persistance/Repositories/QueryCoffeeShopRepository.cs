using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryCoffeeShopRepository(ShopsDbContext dbContext) : IQueryCoffeeShopRepository
{
    private readonly DbSet<CoffeeShop> _repository = dbContext.Shops;
    
    public void Add(CoffeeShop shop)
    {
        _repository.Add(shop);
    }

    public Task<bool> Exists(Guid id, CancellationToken ct = default)
    {
        return _repository.AnyAsync(x => x.Id == id, ct);
    }

    public Task<bool> ExistsByModerationId(Guid id, CancellationToken ct = default)
    {
        return _repository.AnyAsync(x => x.ModerationId == id, ct);
    }

    public async Task<Dictionary<Guid, string>> GetShopNamesByIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default)
    {
        var shopIdList = shopIds.ToList();
        
        var shops = await _repository
            .AsNoTracking()
            .Where(s => shopIdList.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);

        return shops.ToDictionary(s => s.Id, s => s.Name);
    }
}