namespace CoffeePeek.Shops.Domain.Entities;

public class RoasterShop : Entity<Guid>
{
    public Guid RoasterId { get; set; }
    public Guid ShopId { get; set; }
    
    public virtual Roaster Roaster { get; set; }
    public virtual Shop Shop { get; set; }
}