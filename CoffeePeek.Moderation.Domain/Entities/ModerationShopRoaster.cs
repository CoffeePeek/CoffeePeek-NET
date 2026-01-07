using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities;

public sealed class ModerationShopRoaster : Entity<Guid>
{
    public Guid RoasterId { get; private set; }
    public Guid ShopId { get; private set; }
    
    public ModerationShop? ModerationShop { get; private set; }
    
    // ReSharper disable once UnusedMember.Local
    private ModerationShopRoaster()
    {
        
    }
    
    public ModerationShopRoaster(Guid shopId, Guid roasterId)
    {
        ShopId = shopId;
        RoasterId = roasterId;
    }
}