namespace CoffeePeek.ShopsService.Entities;

public class CoffeeBean
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public virtual ICollection<CoffeeBeanShop> CoffeeBeanShops { get; set; } = new HashSet<CoffeeBeanShop>();
}