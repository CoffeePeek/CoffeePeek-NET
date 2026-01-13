using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Entities;

public class BrewMethod : Entity<Guid>
{
    public string Name { get; private set; }
    
    public virtual ICollection<ShopBrewMethod> ShopBrewMethods { get; set; } = new HashSet<ShopBrewMethod>();
}