namespace CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

public interface ICoffeeShopRepository
{
    Task<bool> Exists(Guid id, CancellationToken ct = default);
}