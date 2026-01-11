using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Application.Common;
using CoffeePeek.Shops.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Infrastructure.Services;

public class ShopCacheService(
    IGenericRepository<Shop> shopRepository,
    IHybridCache hybridCache,
    IRedisService redisService) 
    : IShopCacheService
{
    public async Task InvalidateShopCacheAsync(Guid shopId, CancellationToken cancellationToken)
    {
        var cityId = await shopRepository
            .QueryAsNoTracking()
            .Where(s => s.Id == shopId)
            .Select(s => s.CityId)
            .FirstOrDefaultAsync(cancellationToken);

        await hybridCache.RemoveAsync(CacheKey.Shop.Detail(shopId), cancellationToken);
        if (cityId != Guid.Empty)
        {
            await redisService.RemoveByPatternAsync(CacheKey.Shop.ListByCityPattern(cityId));
        }
    }
}