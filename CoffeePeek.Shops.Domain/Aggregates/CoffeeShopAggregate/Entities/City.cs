using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public class City : Entity<Guid>
{
    [MaxLength(50)]
    public string Name { get; private set; }

    public City(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}