using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public class EquipmentCategory : Entity<int>
{
    public string Name { get; private set; } = null!;
}