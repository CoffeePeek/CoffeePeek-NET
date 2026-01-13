using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities;

public class RoasterShop : Entity<Guid>
{
    public Guid RoasterId { get; private set; }
    public Guid ShopId { get; private set; }
    
    public Roaster Roaster { get; private set; }
    public CoffeeShop CoffeeShop { get; private set; }
    
    // ReSharper disable once UnusedMember.Local
    private RoasterShop()
    {
    }

    public RoasterShop(Guid roasterId, Guid shopId)
    {
        RoasterId = roasterId;
        ShopId = shopId;
    }
}