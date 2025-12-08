using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Services.Interfaces;
using MapsterMapper;

namespace CoffeePeek.ShopsService.Services;

public class CacheService(IRedisService redisService, IGenericRepository<City> cityRepository, IMapper mapper) : ICacheService
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
}