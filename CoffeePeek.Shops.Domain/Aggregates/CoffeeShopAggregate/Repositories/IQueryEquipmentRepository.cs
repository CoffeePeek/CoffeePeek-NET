namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public interface IQueryEquipmentRepository
{
    Task<Equipment[]> GetAll(CancellationToken ct = default);
    Task<IEnumerable<Equipment>> GetByIds(List<Guid> ids, CancellationToken cancellationToken);
}