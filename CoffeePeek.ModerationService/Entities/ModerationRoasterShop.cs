namespace CoffeePeek.ModerationService.Entities;

public class ModerationRoasterShop
{
    public Guid Id { get; set; }
    public Guid RoasterId { get; set; }
    public Guid ShopId { get; set; }
    
    public virtual ModerationShop ModerationShop { get; set; }
}