using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Infrastructure.Services;

public class CoffeeShopRepository(IGenericRepository<CoffeeShop> repository, ShopsDbContext dbContext) : ICoffeeShopRepository
{
    public Task<bool> Exists(Guid id, CancellationToken ct = default)
    {
        return repository.AnyAsync(x => x.Id == id, ct);
    }

    public async Task<Dictionary<Guid, string>> GetShopNamesByIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default)
    {
        var shopIdList = shopIds.ToList();
        
        var shops = await repository
            .QueryAsNoTracking()
            .Where(s => shopIdList.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);

        return shops.ToDictionary(s => s.Id, s => s.Name);
    }

    public async Task<UserShopEnrichment> GetUserShopEnrichmentAsync(Guid userId, Guid shopId, CancellationToken ct = default)
    {
        var result = await dbContext.Shops
            .AsNoTracking()
            .Where(s => s.Id == shopId)
            .Select(_ => new
            {
                IsFavorite = dbContext.UserFavorites
                    .Any(f => f.UserId == userId && f.CoffeeShopId == shopId),
                IsVisited = dbContext.CheckIns
                    .Any(c => c.UserId == userId && c.ShopId == shopId),
                ExistingReviewId = dbContext.Reviews
                    .Where(r => r.CoffeeShopId == shopId && r.UserId == userId && !r.IsSoftDelete)
                    .Select(r => (Guid?)r.Id)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        return result != null
            ? new UserShopEnrichment(result.IsFavorite, result.IsVisited, result.ExistingReviewId)
            : new UserShopEnrichment(false, false, null);
    }

    public Task<Dictionary<Guid, UserShopEnrichment>> GetBatchUserShopEnrichmentAsync(Guid userId, IEnumerable<Guid> shopIds,
        CancellationToken ct = default)
    {
        return dbContext.Shops
            .AsNoTracking()
            .Where(s => shopIds.Contains(s.Id))
            .Select(c => new
            {
                c.Id,
                IsFavorite = dbContext.UserFavorites
                    .Any(f => f.UserId == userId && shopIds.Contains(f.CoffeeShopId)),
                IsVisited = dbContext.CheckIns
                    .Any(checkIn => checkIn.UserId == userId && shopIds.Contains(checkIn.ShopId)),
                ExistingReviewId = dbContext.Reviews
                    .Where(r => shopIds.Contains(r.CoffeeShopId) && r.UserId == userId && !r.IsSoftDelete)
                    .Select(r => (Guid?)r.Id)
                    .FirstOrDefault()
            })
            .ToDictionaryAsync(s => s.Id,
                s => new UserShopEnrichment(s.IsFavorite, s.IsVisited, s.ExistingReviewId), cancellationToken: ct);
    }
}