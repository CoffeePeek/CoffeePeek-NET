using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

public class EquipmentCategory : Entity<int>
{
    public string Name { get; private set; } = null!;
}