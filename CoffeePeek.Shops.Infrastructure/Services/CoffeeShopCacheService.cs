using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Application.Common;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Infrastructure.Services;

public class CoffeeShopCacheService(
    IGenericRepository<CoffeeShop> shopRepository,
    IRedisService redisService) 
    : ICoffeeShopCacheService
{
    public async Task InvalidateShopCacheAsync(Guid shopId, CancellationToken cancellationToken)
    {
        var cityId = await shopRepository
            .QueryAsNoTracking()
            .Where(s => s.Id == shopId)
            .Select(s => s.CityId)
            .FirstOrDefaultAsync(cancellationToken);

        await redisService.RemoveAsync(CacheKey.Shop.Detail(shopId));
        if (cityId != Guid.Empty)
        {
            await redisService.RemoveByPattern(CacheKey.Shop.ListByCityPattern(cityId));
        }
    }
}