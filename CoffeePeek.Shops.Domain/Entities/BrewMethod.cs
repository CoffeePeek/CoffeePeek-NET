using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities;

public class BrewMethod : Entity<Guid>
{
    public string Name { get; private set; }
    
    public IReadOnlyCollection<CoffeeShop>? CoffeeShops = new HashSet<CoffeeShop>();
}