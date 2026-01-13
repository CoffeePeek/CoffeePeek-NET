using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities;

public class ShopBrewMethod : Entity<Guid>
{
    public Guid BrewMethodId { get; set; }
    public Guid ShopId { get; set; }

    public virtual BrewMethod BrewMethod { get; set; }
    public virtual CoffeeShop CoffeeShop { get; set; }

    private ShopBrewMethod()
    {
    }

    public ShopBrewMethod(Guid brewMethodId, Guid shopId)
    {
        BrewMethodId = brewMethodId;
        ShopId = shopId;
    }
}