namespace CoffeePeek.Shops.Domain.Entities;

public class BrewMethod
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public virtual ICollection<ShopBrewMethod> ShopBrewMethods { get; set; } = new HashSet<ShopBrewMethod>();
}