using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Services.Interfaces;
using MapsterMapper;

namespace CoffeePeek.ShopsService.Services;

public class CacheService(IRedisService redisService, IGenericRepository<City> cityRepository, IMapper mapper) : ICacheService
{
    private static readonly TimeSpan LongTimeout = TimeSpan.FromDays(5);
    
    public async Task<CityDto[]> GetCities()
    {
        const string key = $"{nameof(CityDto)}s";

        var result = await redisService.TryGetAsync<CityDto[]>(key);
        
        if (!result.success)
        {
            var cities = await cityRepository.GetAllAsNoTrackingAsync();
            result.value = mapper.Map<CityDto[]>(cities);
            await redisService.SetAsync(key, result.value, LongTimeout);
        }

        return result.value;
    }
}