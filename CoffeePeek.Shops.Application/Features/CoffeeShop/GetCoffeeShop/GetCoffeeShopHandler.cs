using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

public class GetCoffeeShopHandler(
    IGenericRepository<Domain.Entities.CoffeeShopAggregate.CoffeeShop> shopRepository,
    IMapper mapper,
    IRedisService redisService)
    : IRequestHandler<GetCoffeeShopQuery, Response<GetCoffeeShopResponse>>
{
    public async Task<Response<GetCoffeeShopResponse>> Handle(GetCoffeeShopQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.Shop.Detail(request.Id);
        
        var result = await redisService.GetAsync(
            cacheKey,
            async () =>
            {
                var shop = await shopRepository
                    .QueryAsNoTracking()
                    .Include(x => x.ShopPhotos)
                    .Include(x => x.Reviews)
                    .Include(x => x.ShopEquipments).ThenInclude(x => x.Equipment)
                    .Include(x => x.CoffeeBeanShops).ThenInclude(x => x.CoffeeBean)
                    .Include(x => x.RoasterShops).ThenInclude(x => x.Roaster)
                    .Include(x => x.ShopBrewMethods).ThenInclude(x => x.BrewMethod)
                    .Include(x => x.Contact)
                    .Include(x => x.Location)
                    .Include(x => x.Schedules)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                var shopDto = shop?.Adapt<CoffeeShopDetailsDto>(mapper.Config);

                return shopDto == null
                    ? Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {request.Id} not found.")
                    : Response<GetCoffeeShopResponse>.Success(new GetCoffeeShopResponse(shopDto));
            },
            cacheKey.DefaultTtl);

        return result ?? Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {request.Id} not found.");
    }
}