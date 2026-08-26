using CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryCoffeeDrinkRepository(ShopsDbContext dbContext) : IQueryCoffeeDrinkRepository
{
    public Task<CoffeeDrinkDefinition[]> GetActiveAsync(CancellationToken ct = default) =>
        dbContext.CoffeeDrinkDefinitions
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.NameEn)
            .ToArrayAsync(ct);
}

public class QueryShopMenuRepository(ShopsDbContext dbContext) : IQueryShopMenuRepository
{
    public Task<ShopMenu?> GetByShopIdAsync(Guid shopId, CancellationToken ct = default) =>
        dbContext.ShopMenus
            .AsNoTracking()
            .Include(m => m.Items)
            .ThenInclude(i => i.DrinkDefinition)
            .Include(m => m.Photos)
            .FirstOrDefaultAsync(m => m.CoffeeShopId == shopId, ct);
}

public class ShopMenuRepository(ShopsDbContext dbContext) : IShopMenuRepository
{
    public Task<ShopMenu?> GetTrackedByShopIdAsync(Guid shopId, CancellationToken ct = default) =>
        dbContext.ShopMenus
            .Include(m => m.Items)
            .Include(m => m.Photos)
            .FirstOrDefaultAsync(m => m.CoffeeShopId == shopId, ct);

    public void Add(ShopMenu menu) => dbContext.ShopMenus.Add(menu);
}
