namespace CoffeePeek.ModerationService.Entities;

public class ModerationShopBrewMethod
{
    public Guid Id { get; set; }
    public Guid BrewMethodId { get; set; }
    public Guid ShopId { get; set; }
    
    public virtual ModerationShop ModerationShop { get; set; }
}





