using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public class Roaster : Entity<Guid>
{
    public string Name { get; private set; }
    public ICollection<CoffeeShop> CoffeeShops { get; private set; } = new HashSet<CoffeeShop>();
}