using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryCoffeeBeanRepository(ShopsDbContext dbContext) : IQueryCoffeeBeanRepository
{
    private readonly DbSet<CoffeeBean> _repository = dbContext.CoffeeBeans;
    
    public Task<CoffeeBean[]> GetAll(CancellationToken ct = default)
    {
        return _repository.AsNoTracking().ToArrayAsync(ct);
    }

    public async Task<IEnumerable<CoffeeBean>> GetByIds(List<Guid> ids, CancellationToken ct)
    {
        return await _repository.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);
    }
}