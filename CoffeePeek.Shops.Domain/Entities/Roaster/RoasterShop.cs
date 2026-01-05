using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Entities;

public class RoasterShop : Entity<Guid>
{
    public Guid RoasterId { get; private set; }
    public Guid ShopId { get; private set; }
    
    public Roaster Roaster { get; private set; }
    public Shop Shop { get; private set; }
    
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