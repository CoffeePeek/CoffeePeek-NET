namespace CoffeePeek.Shops.Domain.Entities;

public class CoffeeBean : Entity<Guid>
{
    public string Name { get; set; }

    public virtual ICollection<CoffeeBeanShop> CoffeeBeanShops { get; set; } = new HashSet<CoffeeBeanShop>();
}