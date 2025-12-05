namespace CoffeePeek.ShopsService.Entities;

public class ShopBrewMethod
{
    public Guid Id { get; set; }
    public Guid BrewMethodId { get; set; }
    public Guid ShopId { get; set; }
    
    public virtual BrewMethod BrewMethod { get; set; }
    public virtual Shop Shop { get; set; }
}