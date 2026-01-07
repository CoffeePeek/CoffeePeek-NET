using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities;

public sealed class ModerationCoffeeBeanShop : Entity<Guid>
{
    public Guid ShopId { get; private set; }
    public Guid CoffeeBeanId { get; private set; }

    public ModerationShop? ModerationShop { get; private set; }

    private ModerationCoffeeBeanShop()
    {
        
    }
    
    public ModerationCoffeeBeanShop(Guid shopId, Guid coffeeBeanId)
    {
        ShopId = shopId;
        CoffeeBeanId = coffeeBeanId;
    }
}