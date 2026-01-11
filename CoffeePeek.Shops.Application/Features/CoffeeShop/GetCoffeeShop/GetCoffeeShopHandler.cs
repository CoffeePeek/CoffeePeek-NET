using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Domain.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

public class GetCoffeeShopHandler(
    IGenericRepository<Shop> shopRepository,
    IMapper mapper,
    IHybridCache hybridCache)
    : IRequestHandler<GetCoffeeShopQuery, Response<GetCoffeeShopResponse>>
{
    public async Task<Response<GetCoffeeShopResponse>> Handle(GetCoffeeShopQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.Shop.Detail(request.Id);
        
        var result = await hybridCache.GetOrSetAsync(
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
                    .Include(x => x.ShopContact)
                    .Include(x => x.Location)
                    .Include(x => x.Schedules)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                var shopDto = shop?.Adapt<CoffeeShopDetailsDto>(mapper.Config);

                return shopDto == null
                    ? Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {request.Id} not found.")
                    : Response<GetCoffeeShopResponse>.Success(new GetCoffeeShopResponse(shopDto));
            },
            distributedTtl: cacheKey.DefaultTtl,
            memoryTtl: TimeSpan.FromMinutes(1),
            ct: cancellationToken);

        return result ?? Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {request.Id} not found.");
    }
}