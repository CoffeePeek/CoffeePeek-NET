using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities;

public class Roaster : Entity<Guid>
{
    public string Name { get; private set; }
    public ICollection<CoffeeShop> CoffeeShops { get; private set; } = new HashSet<CoffeeShop>();
}