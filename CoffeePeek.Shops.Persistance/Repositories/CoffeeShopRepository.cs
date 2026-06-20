using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.Repositories;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class CoffeeShopRepository(ShopsDbContext dbContext) : ICoffeeShopRepository
{
    public Task<CoffeeShop?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return dbContext.Shops.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public Task<CoffeeShop?> GetByIdForOwnerAsync(Guid id, Guid ownerUserId, CancellationToken ct = default)
    {
        return dbContext.Shops.FirstOrDefaultAsync(s => s.Id == id && s.OwnerUserId == ownerUserId, ct);
    }
}

public class AdminCoffeeShopQueryRepository(ShopsDbContext dbContext) : IAdminCoffeeShopQueryRepository
{
    public async Task<(IReadOnlyList<CoffeeShop> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CoffeeShopStatus? status,
        CancellationToken ct = default)
    {
        var query = dbContext.Shops.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                s.Location.Address.ToLower().Contains(term));
        }

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<CoffeeShop>> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken ct = default)
    {
        return await dbContext.Shops
            .AsNoTracking()
            .Where(s => s.OwnerUserId == ownerUserId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }
}
