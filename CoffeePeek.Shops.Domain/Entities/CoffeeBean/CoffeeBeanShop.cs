namespace CoffeePeek.Shops.Domain.Entities;

public class CoffeeBeanShop : Entity<Guid>
{
    
    public Guid ShopId { get; set; }
    public Guid CoffeeBeanId { get; set; }

    public virtual CoffeeBean CoffeeBean { get; set; }
    public virtual Shop Shop { get; set; }
}