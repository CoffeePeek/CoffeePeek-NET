using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.BrewMethods;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public class Beverages : Entity<Guid>
{
    public string Name  { get; private set; }
    public Guid BrewMethodId { get; private set; }
    
    public BrewMethod BrewMethod { get; private set; }
}