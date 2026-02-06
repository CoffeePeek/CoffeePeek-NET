using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Aggregates.BrewMethods;

public class BrewMethod : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public BrewMethodCategory Category { get; private set; }
    
    public IReadOnlyCollection<CoffeeShop>? CoffeeShops = new HashSet<CoffeeShop>();
}