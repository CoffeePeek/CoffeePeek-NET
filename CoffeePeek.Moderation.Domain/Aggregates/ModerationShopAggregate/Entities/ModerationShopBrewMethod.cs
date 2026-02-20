using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Moderation.Domain.Aggregates;

public class ModerationShopBrewMethod : Entity<Guid>
{
    public Guid BrewMethodId { get; private set; }
    public Guid ShopId { get; private set; }
    
    public ModerationShop? ModerationShop { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private ModerationShopBrewMethod() { }
    
    public ModerationShopBrewMethod(Guid shopId, Guid brewMethodId)
    {
        ShopId = shopId;
        BrewMethodId = brewMethodId;
    }
}