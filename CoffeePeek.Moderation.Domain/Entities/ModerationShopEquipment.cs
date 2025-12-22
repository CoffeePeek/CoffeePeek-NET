namespace CoffeePeek.Moderation.Domain.Entities;

public class ModerationShopEquipment
{
    public Guid Id { get; set; }
    
    public Guid ShopId { get; set; }
    public Guid EquipmentId { get; set; }
    
    public virtual ModerationShop ModerationShop { get; set; }
}