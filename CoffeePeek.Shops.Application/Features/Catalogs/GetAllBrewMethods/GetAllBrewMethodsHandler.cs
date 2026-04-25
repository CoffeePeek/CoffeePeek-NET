using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllBrewMethods;

public class GetAllBrewMethodsHandler
{
    public async Task<Response<GetAllBrewMethodsResponse>> Handle(
        GetAllBrewMethodsCommand command, 
        ICacheService cacheService, 
        IQueryBrewMethodRepository repository,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.BrewMethod.ListAll();
        
        var result = await cacheService.GetAsync<BrewMethodDto[]>(cacheKey);

        if (result == null)
        {
            var cities = await repository.GetAll(cancellationToken);
            result = mapper.Map<BrewMethodDto[]>(cities);
            await cacheService.SetAsync(cacheKey, result);
        }

        var response = new GetAllBrewMethodsResponse(result);
        
        return Response<GetAllBrewMethodsResponse>.Success(response);
    }
}