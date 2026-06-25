using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryCoffeeShopRepository(ShopsDbContext dbContext) : IQueryCoffeeShopRepository
{
    private readonly DbSet<CoffeeShop> _repository = dbContext.Shops;
    
    public void Add(CoffeeShop shop)
    {
        AttachCatalogReferencesAsUnchanged(shop);
        _repository.Add(shop);
    }

    private void AttachCatalogReferencesAsUnchanged(CoffeeShop shop)
    {
        foreach (var brewMethod in shop.BrewMethods)
            dbContext.Entry(brewMethod).State = EntityState.Unchanged;

        foreach (var roaster in shop.Roasters)
            dbContext.Entry(roaster).State = EntityState.Unchanged;

        foreach (var coffeeBean in shop.CoffeeBeans)
            dbContext.Entry(coffeeBean).State = EntityState.Unchanged;

        foreach (var equipment in shop.Equipments)
            dbContext.Entry(equipment).State = EntityState.Unchanged;
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

    public async Task<HashSet<Guid>> GetShopIdsByCityIdAsync(Guid cityId, CancellationToken ct = default)
    {
        var shopIds = await _repository
            .AsNoTracking()
            .Where(s => s.Location.CityId == cityId)
            .Select(s => s.Id)
            .ToListAsync(ct);

        return shopIds.ToHashSet();
    }
}