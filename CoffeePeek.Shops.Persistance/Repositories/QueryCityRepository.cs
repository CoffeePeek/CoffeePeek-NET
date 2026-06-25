using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryCityRepository(ShopsDbContext dbContext) : IQueryCityRepository
{
    private readonly DbSet<City> _repository = dbContext.Cities;
    
    public Task<City[]> GetAll(CancellationToken ct = default)
    {
        return _repository.AsNoTracking().ToArrayAsync(ct);
    }

    public Task<bool> Exists(Guid cityId, CancellationToken ct = default) =>
        _repository.AsNoTracking().AnyAsync(c => c.Id == cityId, ct);
}