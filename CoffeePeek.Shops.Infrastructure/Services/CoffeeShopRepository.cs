using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Infrastructure.Services;

public class CoffeeShopRepository(IGenericRepository<CoffeeShop> repository) : ICoffeeShopRepository
{
    public Task<bool> Exists(Guid id, CancellationToken ct = default)
    {
        return repository.AnyAsync(x => x.Id == id, ct);
    }
}