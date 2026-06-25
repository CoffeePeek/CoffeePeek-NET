using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Common.Enums;
using CoffeeShop.Moderation.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Moderation.Persistence.Repositories;

public class QueryModerationShopRepository(ModerationDbContext dbContext) : IQueryModerationShopRepository
{
    private readonly DbSet<ModerationShop> _repository = dbContext.ModerationShops;

    public Task<ModerationShop?> GetByPublishedShopId(Guid publishedShopId, CancellationToken ct)
    {
        return _repository.FirstOrDefaultAsync(x => x.ShopId == publishedShopId, ct);
    }

    public async Task<IReadOnlyList<ModerationShop>> GetAllForReviewAsync(CancellationToken cancellationToken = default)
    {
        return await BuildReviewQuery().ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<ModerationShop> Items, int TotalCount)> GetPagedForReviewAsync(
        int page,
        int pageSize,
        ModerationStatus? status,
        string? search,
        CancellationToken ct = default)
    {
        var query = BuildReviewQuery();

        if (status.HasValue)
            query = query.Where(s => s.ModerationStatus == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                (s.Location != null && s.Location.Address.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    private IQueryable<ModerationShop> BuildReviewQuery() =>
        _repository.AsNoTracking()
            .Include(s => s.Contact)
            .Include(s => s.Location)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
            .ThenInclude(sch => sch.Intervals)
            .Include(s => s.ModerationShopEquipments)
            .Include(s => s.ModerationCoffeeBeanShops)
            .Include(s => s.ModerationRoasterShops)
            .Include(s => s.ModerationShopBrewMethods);
}