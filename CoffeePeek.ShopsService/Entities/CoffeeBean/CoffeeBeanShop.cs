namespace CoffeePeek.ShopsService.Entities;

public class CoffeeBeanShop
{
    public Guid Id { get; set; }
    
    public Guid ShopId { get; set; }
    public Guid CoffeeBeanId { get; set; }

    public virtual CoffeeBean CoffeeBean { get; set; }
    public virtual Shop Shop { get; set; }
}