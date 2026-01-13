using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities;

public class CoffeeBeanShop : Entity<Guid>
{
    public Guid ShopId { get; private set; }
    public Guid CoffeeBeanId { get; private set; }

    public CoffeeBean CoffeeBean { get; private set; }
    public CoffeeShop CoffeeShop { get; private set; }

    private CoffeeBeanShop()
    {
        
    }

    public CoffeeBeanShop(Guid coffeeBeanId, Guid id)
    {
        CoffeeBeanId = coffeeBeanId;
        ShopId = id;
    }
}