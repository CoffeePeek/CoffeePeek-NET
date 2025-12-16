using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Cache;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Services.Interfaces;
using MapsterMapper;

namespace CoffeePeek.ShopsService.Services;

public class CacheService(
    IHybridCache cache,
    ICacheInvalidationStrategy cacheInvalidationStrategy,
    IGenericRepository<City> cityRepository,
    IGenericRepository<CoffeeBean> coffeeBeanRepository,
    IGenericRepository<Equipment> equipmentRepository,
    IGenericRepository<Roaster> roasterRepository,
    IGenericRepository<BrewMethod> brewMethodRepository,
    IMapper mapper) : ICacheService
{
    public async Task<CityDto[]?> GetCities()
    {
        var cacheKey = CacheKey.Shop.Cities();
        return await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var cities = await cityRepository.GetAllAsNoTrackingAsync();
                return mapper.Map<CityDto[]>(cities);
            },
            distributedTtl: cacheKey.DefaultTtl);
    }

    public async Task<BeansDto[]?> GetBeans()
    {
        var cacheKey = CacheKey.Shop.Beans();
        return await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var beans = await coffeeBeanRepository.GetAllAsNoTrackingAsync();
                return mapper.Map<BeansDto[]>(beans);
            },
            distributedTtl: cacheKey.DefaultTtl);
    }

    public async Task<EquipmentDto[]?> GetEquipments()
    {
        var cacheKey = CacheKey.Shop.Equipment();
        return await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var equipments = await equipmentRepository.GetAllAsNoTrackingAsync();
                return mapper.Map<EquipmentDto[]>(equipments);
            },
            distributedTtl: cacheKey.DefaultTtl);
    }

    public async Task<RoasterDto[]?> GetRoasters()
    {
        var cacheKey = CacheKey.Shop.Roasters();
        return await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var roasters = await roasterRepository.GetAllAsNoTrackingAsync();
                return mapper.Map<RoasterDto[]>(roasters);
            },
            distributedTtl: cacheKey.DefaultTtl);
    }

    public async Task<BrewMethodDto[]?> GetBrewMethods()
    {
        var cacheKey = CacheKey.Shop.BrewMethods();
        return await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var brewMethods = await brewMethodRepository.GetAllAsNoTrackingAsync();
                return mapper.Map<BrewMethodDto[]>(brewMethods);
            },
            distributedTtl: cacheKey.DefaultTtl);
    }

    public Task InvalidateShopDictionaries(CancellationToken cancellationToken = default)
    {
        return cacheInvalidationStrategy.InvalidateTagsAsync([CacheInvalidationStrategy.Tags.ShopsDictionary], cancellationToken);
    }

    public Task InvalidateShopLists(CancellationToken cancellationToken = default)
    {
        return cacheInvalidationStrategy.InvalidateTagsAsync([CacheInvalidationStrategy.Tags.ShopsLists], cancellationToken);
    }
}