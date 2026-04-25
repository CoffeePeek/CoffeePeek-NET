using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllCities;

public class GetCitiesRequestHandler
{
    public async Task<Response<GetCitiesResponse>> Handle(
        GetCitiesCommand command, 
        ICacheService redisService, 
        IMapper mapper,
        IQueryCityRepository queryCityRepository, 
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.City.ListAll();
        
        var result = await redisService.GetAsync<CityDto[]>(cacheKey);

        if (result == null)
        {
            var cities = await queryCityRepository.GetAll(cancellationToken);
            result = mapper.Map<CityDto[]>(cities);
            await redisService.SetAsync(cacheKey, result);
        }

        var response = new GetCitiesResponse(result);

        return Response<GetCitiesResponse>.Success(response);
    }
}