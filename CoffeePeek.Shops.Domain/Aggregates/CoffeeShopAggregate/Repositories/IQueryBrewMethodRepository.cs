using CoffeePeek.Shops.Domain.Aggregates.BrewMethods;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public interface IQueryBrewMethodRepository
{
    Task<BrewMethod[]> GetAll(CancellationToken ct = default);
    Task<IEnumerable<BrewMethod>> GetByIds(List<Guid> ids, CancellationToken cancellationToken);
}