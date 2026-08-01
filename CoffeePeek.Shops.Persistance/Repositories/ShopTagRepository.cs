using CoffeePeek.Shops.Domain.Aggregates.ShopTagAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class ShopTagRepository(ShopsDbContext dbContext) : IShopTagRepository
{
    public Task<ShopTag?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.ShopTags.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<ShopTag?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var normalized = ShopTag.NormalizeSlug(slug);
        return dbContext.ShopTags.FirstOrDefaultAsync(t => t.Slug == normalized, ct);
    }

    public void Add(ShopTag tag) => dbContext.ShopTags.Add(tag);
}

public class QueryShopTagRepository(ShopsDbContext dbContext) : IQueryShopTagRepository
{
    public Task<ShopTag[]> GetAllAsync(CancellationToken ct = default) =>
        dbContext.ShopTags
            .AsNoTracking()
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToArrayAsync(ct);

    public Task<ShopTag[]> GetActiveAsync(CancellationToken ct = default) =>
        dbContext.ShopTags
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToArrayAsync(ct);

    public async Task<bool> AllExistAndActiveAsync(IReadOnlyCollection<Guid> tagIds, CancellationToken ct = default)
    {
        if (tagIds.Count == 0)
            return true;

        var distinct = tagIds.Distinct().ToArray();
        var count = await dbContext.ShopTags
            .AsNoTracking()
            .CountAsync(t => distinct.Contains(t.Id) && t.IsActive, ct);

        return count == distinct.Length;
    }
}
