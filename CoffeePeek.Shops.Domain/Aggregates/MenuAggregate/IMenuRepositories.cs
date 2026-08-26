namespace CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;

public interface IQueryCoffeeDrinkRepository
{
    Task<CoffeeDrinkDefinition[]> GetActiveAsync(CancellationToken ct = default);
}

public interface IQueryShopMenuRepository
{
    Task<ShopMenu?> GetByShopIdAsync(Guid shopId, CancellationToken ct = default);
}

public interface IShopMenuRepository
{
    Task<ShopMenu?> GetTrackedByShopIdAsync(Guid shopId, CancellationToken ct = default);
    void Add(ShopMenu menu);
}
