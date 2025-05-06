using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Domain.UnitOfWork;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using MapsterMapper;

namespace CoffeePeek.Infrastructure.Cache;

public class CacheService(IRedisService redisService, 
    IMapper mapper,
    IRepository<City> cityRepository) 
    : ICacheService
{
    private static readonly TimeSpan ShortTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MiddleTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LongTimeout = TimeSpan.FromDays(5);

    public async Task<ICollection<CityDto>> GetCities()
    {
        var key = $"{nameof(CityDto)}s";

        var result = await redisService.TryGetAsync<ICollection<CityDto>>(key);
        
        if (!result.success)
        {
            var cities = await cityRepository.GetAllAsync();
            result.value = mapper.Map<ICollection<CityDto>>(cities);
            await redisService.SetAsync(key, result.value, LongTimeout);
        }

        return result.value;
    }
}