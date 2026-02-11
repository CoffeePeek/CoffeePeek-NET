using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryEquipmentRepository(ShopsDbContext dbContext) : IQueryEquipmentRepository
{
    private readonly DbSet<Equipment> _repository = dbContext.Equipments;
    
    public Task<Equipment[]> GetAll(CancellationToken ct = default)
    {
        return _repository.AsNoTracking().ToArrayAsync(ct);
    }

    public async Task<IEnumerable<Equipment>> GetByIds(List<Guid> ids, CancellationToken ct)
    {
        return await _repository.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);
    }
}