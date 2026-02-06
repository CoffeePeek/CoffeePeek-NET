namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public interface ICoffeeShopRepository
{
    Task<bool> Exists(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetShopNamesByIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default);
}