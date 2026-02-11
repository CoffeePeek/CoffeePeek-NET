using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Moderation.Domain.Aggregates;

public class ModerationShopEquipment : Entity<Guid>
{
    public Guid ShopId { get; private set; }
    public Guid EquipmentId { get; private set; }
    
    public Aggregates.ModerationShop? ModerationShop { get; private set; }

    private ModerationShopEquipment()
    {
        
    }

    public ModerationShopEquipment(Guid shopId, Guid equipmentId)
    {
        ShopId = shopId;
        EquipmentId = equipmentId;
    }
}