using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Services.Interfaces;
using MapsterMapper;

namespace CoffeePeek.ShopsService.Services;

public class CacheService(
    IRedisService redisService, 
    IGenericRepository<City> cityRepository,
    IGenericRepository<CoffeeBean> coffeeBeanRepository,
    IGenericRepository<Equipment> equipmentRepository,
    IMapper mapper) : ICacheService
{
    public async Task<CityDto[]> GetCities()
    {
        var cacheKey = CacheKey.Shop.Cities();
        var cachedCities = await redisService.GetAsync<CityDto[]>(cacheKey);
        
        if (cachedCities != null)
        {
            return cachedCities;
        }
        
        var cities = await cityRepository.GetAllAsNoTrackingAsync();
        var citiesDto = mapper.Map<CityDto[]>(cities);
        
        await redisService.SetAsync(cacheKey, citiesDto);

        return citiesDto;
    }

    public async Task<BeansDto[]> GetBeans()
    {
        var cacheKey = CacheKey.Shop.Beans();
        var cachedBeans = await redisService.GetAsync<BeansDto[]>(cacheKey);
        
        if (cachedBeans != null)
        {
            return cachedBeans;
        }
        
        var beans = await coffeeBeanRepository.GetAllAsNoTrackingAsync();
        var beansDto = mapper.Map<BeansDto[]>(beans);
        
        await redisService.SetAsync(cacheKey, beansDto, TimeSpan.FromDays(1));

        return beansDto;
    }

    public async Task<EquipmentDto[]> GetEquipments()
    {
        var cacheKey = CacheKey.Shop.Equipment();
        var cachedEquipments = await redisService.GetAsync<EquipmentDto[]>(cacheKey);
        
        if (cachedEquipments != null)
        {
            return cachedEquipments;
        }
        
        var equipments = await equipmentRepository.GetAllAsNoTrackingAsync();
        var equipmentsDto = mapper.Map<EquipmentDto[]>(equipments);
        
        await redisService.SetAsync(cacheKey, equipmentsDto, TimeSpan.FromDays(1));

        return equipmentsDto;
    }
}