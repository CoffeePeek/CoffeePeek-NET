namespace CoffeePeek.ModerationService.Entities;

public class ModerationCoffeeBeanShop
{
    public Guid Id { get; set; }
    
    public Guid ShopId { get; set; }
    public Guid CoffeeBeanId { get; set; }

    public virtual ModerationShop ModerationShop { get; set; }
}





