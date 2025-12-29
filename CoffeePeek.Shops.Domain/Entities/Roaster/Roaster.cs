namespace CoffeePeek.Shops.Domain.Entities;

public class Roaster : Entity<Guid>
{
    public required string Name { get; set; }
    
    public virtual ICollection<RoasterShop> RoasterShops { get; set; } = new HashSet<RoasterShop>();
}