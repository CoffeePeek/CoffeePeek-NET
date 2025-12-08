namespace CoffeePeek.ShopsService.Entities;

public class RoasterShop
{
    public Guid Id { get; set; }
    public Guid RoasterId { get; set; }
    public Guid ShopId { get; set; }
    
    public virtual Roaster Roaster { get; set; }
    public virtual Shop Shop { get; set; }
}