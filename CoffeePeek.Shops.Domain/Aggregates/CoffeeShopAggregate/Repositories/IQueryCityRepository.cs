namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public interface IQueryCityRepository
{
    Task<City[]> GetAll(CancellationToken ct = default);
    Task<bool> Exists(Guid cityId, CancellationToken ct = default);
}