namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public interface IQueryCoffeeShopRepository
{
    void Add(CoffeeShop shop);
    Task<bool> Exists(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByModerationId(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetShopNamesByIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default);
    Task<HashSet<Guid>> GetShopIdsByCityIdAsync(Guid cityId, CancellationToken ct = default);
}