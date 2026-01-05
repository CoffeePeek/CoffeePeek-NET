using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Entities;

public class City : Entity<Guid>
{
    public required string Name { get; set; }
}