using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public class CoffeeBean : Entity<Guid>
{
    public string Name { get; private set; }

    public IReadOnlyCollection<CoffeeShop> CoffeeShops { get; private set; } = new HashSet<CoffeeShop>();
}