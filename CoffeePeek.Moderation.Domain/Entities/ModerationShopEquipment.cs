using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities;

public class ModerationShopEquipment : Entity<Guid>
{
    public Guid ShopId { get; private set; }
    public Guid EquipmentId { get; private set; }
    
    public ModerationShop? ModerationShop { get; private set; }

    private ModerationShopEquipment()
    {
        
    }

    public ModerationShopEquipment(Guid shopId, Guid equipmentId)
    {
        ShopId = shopId;
        EquipmentId = equipmentId;
    }
}