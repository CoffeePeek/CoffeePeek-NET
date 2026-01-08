using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Persistence;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Entities;
using MapsterMapper;

namespace CoffeePeek.Shops.Infrastructure.Services;

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
        var cacheKey = CacheKey.City.ListAll();
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
        var cacheKey = CacheKey.Bean.ListAll();
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
        var cacheKey = CacheKey.Equipment.ListAll();
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
        var cacheKey = CacheKey.Roaster.ListAll();
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
        var cacheKey = CacheKey.BrewMethod.ListAll();
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