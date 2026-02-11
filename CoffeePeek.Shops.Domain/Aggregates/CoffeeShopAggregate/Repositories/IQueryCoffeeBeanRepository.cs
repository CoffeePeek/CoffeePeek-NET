namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public interface IQueryCoffeeBeanRepository
{
    Task<CoffeeBean[]> GetAll(CancellationToken ct = default);
    Task<IEnumerable<CoffeeBean>> GetByIds(List<Guid> ids, CancellationToken cancellationToken);
}