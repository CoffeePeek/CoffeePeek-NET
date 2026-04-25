using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public class EquipmentCategory : Entity<int>
{
    public string Name { get; private set; } = null!;
}