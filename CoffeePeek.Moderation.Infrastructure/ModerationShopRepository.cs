using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Moderation.Infrastructure;

public class ModerationShopRepository(ModerationDbContext context) : IModerationShopRepository
{
    private DbSet<ModerationShop> ShopRepository => context.ModerationShops;
    public async Task<bool> Any(ISpecification<ModerationShop> spec, CancellationToken ct = default)
    {
        return await ShopRepository.AnyAsync(spec.Criteria, ct);
    }

    public async Task<ModerationShop?> GetByIdWithOutDetails(Guid moderationShopId, CancellationToken ct = default)
    {
        return await ShopRepository.FirstOrDefaultAsync(x => x.Id == moderationShopId , ct);
    }

    public async Task<IReadOnlyList<ModerationShop>> GetAllForReviewAsync()
    {
        return await ShopRepository
            .Include(s => s.Contact)
            .Include(s => s.Location)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
            .ThenInclude(sch => sch.Intervals)
            .Include(s => s.ModerationShopEquipments)
            .Include(s => s.ModerationCoffeeBeanShops)
            .Include(s => s.ModerationRoasterShops)
            .Include(s => s.ModerationShopBrewMethods)
            .ToListAsync();
    }

    public async Task<ModerationShop?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await ShopRepository
            .Include(s => s.Contact)
            .Include(s => s.Location)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
                .ThenInclude(sch => sch.Intervals)
            .Include(s => s.ModerationShopEquipments)
            .Include(s => s.ModerationCoffeeBeanShops)
            .Include(s => s.ModerationRoasterShops)
            .Include(s => s.ModerationShopBrewMethods)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(ModerationShop shop)
    {
        ArgumentNullException.ThrowIfNull(shop);
        await ShopRepository.AddAsync(shop);
    }

}


