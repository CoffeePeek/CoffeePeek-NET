using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryRoasterRepository(ShopsDbContext dbContext) : IQueryRoasterRepository
{
    private readonly DbSet<Roaster> _repository = dbContext.Roasters;
    
    public Task<Roaster[]> GetAll()
    {
        return _repository.AsNoTracking().ToArrayAsync();
    }

    public async Task<IEnumerable<Roaster>> GetByIds(List<Guid> ids, CancellationToken ct)
    {
        return await _repository.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);
    }
}