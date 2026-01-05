using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Entities;

public class ShopBrewMethod : Entity<Guid>
{
    public Guid BrewMethodId { get; set; }
    public Guid ShopId { get; set; }

    public virtual BrewMethod BrewMethod { get; set; }
    public virtual Shop Shop { get; set; }

    private ShopBrewMethod()
    {
    }

    public ShopBrewMethod(Guid brewMethodId, Guid shopId)
    {
        BrewMethodId = brewMethodId;
        ShopId = shopId;
    }
}