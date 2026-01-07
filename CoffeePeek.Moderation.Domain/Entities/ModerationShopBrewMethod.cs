using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities;

public class ModerationShopBrewMethod : Entity<Guid>
{
    public Guid BrewMethodId { get; private set; }
    public Guid ShopId { get; private set; }
    
    public ModerationShop? ModerationShop { get; private set; }

    private ModerationShopBrewMethod()
    {
        
    }
    
    public ModerationShopBrewMethod(Guid shopId, Guid brewMethodId)
    {
        ShopId = shopId;
        BrewMethodId = brewMethodId;
    }
}