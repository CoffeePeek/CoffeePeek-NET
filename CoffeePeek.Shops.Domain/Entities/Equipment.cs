using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities;

public class Equipment : Entity<Guid>
{
    public string Name { get; private set; }

    public IReadOnlyCollection<CoffeeShop> CoffeeShops { get; private set; } = new HashSet<CoffeeShop>();
}