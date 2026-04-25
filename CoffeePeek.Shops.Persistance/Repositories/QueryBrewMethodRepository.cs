using CoffeePeek.Shops.Domain.Aggregates.BrewMethods;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryBrewMethodRepository(ShopsDbContext dbContext) : IQueryBrewMethodRepository
{
    private readonly DbSet<BrewMethod> _repository = dbContext.BrewMethods;
    
    public Task<BrewMethod[]> GetAll(CancellationToken ct = default)
    {
        return _repository.AsNoTracking().ToArrayAsync(ct);
    }

    public async Task<IEnumerable<BrewMethod>> GetByIds(List<Guid> ids, CancellationToken ct)
    {
        return await _repository.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);
    }
}