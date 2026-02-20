using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetAllBeans;

public class GetAllBeansHandler
{
    public async Task<Response<GetAllCoffeeBeansResponse>> Handle(GetAllBeansCommand _, 
        ICacheService cacheService,
        IQueryCoffeeBeanRepository repository,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.CoffeeBean.ListAll();
        
        var result = await cacheService.GetAsync<CoffeeBeansDto[]>(cacheKey);
        
        if (result == null)
        {
            var coffeeBeans = await repository.GetAll(cancellationToken);
            result = mapper.Map<CoffeeBeansDto[]>(coffeeBeans);
            await cacheService.SetAsync(cacheKey, result);
        }
        
        var response = new GetAllCoffeeBeansResponse(result);

        return Response<GetAllCoffeeBeansResponse>.Success(response);
    }
}