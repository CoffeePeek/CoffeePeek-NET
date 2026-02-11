using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeeShop.Moderation.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Moderation.Persistence.Repositories;

public class QueryModerationShopRepository(ModerationDbContext dbContext) : IQueryModerationShopRepository
{
    private readonly DbSet<ModerationShop> _repository = dbContext.ModerationShops;
    public Task<ModerationShop?> GetById(Guid shopId, CancellationToken ct)
    {
        return _repository.FirstOrDefaultAsync(x => x.Id == shopId, ct);
    }
    
    public async Task<IReadOnlyList<ModerationShop>> GetAllForReviewAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.AsNoTracking()
            .Include(s => s.Contact)
            .Include(s => s.Location)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
            .ThenInclude(sch => sch.Intervals)
            .Include(s => s.ModerationShopEquipments)
            .Include(s => s.ModerationCoffeeBeanShops)
            .Include(s => s.ModerationRoasterShops)
            .Include(s => s.ModerationShopBrewMethods)
            .ToListAsync(cancellationToken);
    }
}