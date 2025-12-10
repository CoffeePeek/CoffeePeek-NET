using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.ShopsService.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop;

public class GetCoffeeShopHandler(
    IGenericRepository<Shop> shopRepository,
    IMapper mapper,
    IRedisService redisService)
    : IRequestHandler<GetCoffeeShopCommand, Response<GetCoffeeShopResponse>>
{
    public async Task<Response<GetCoffeeShopResponse>> Handle(GetCoffeeShopCommand request,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.Shop.ById(request.Id);
        var cached = await redisService.GetAsync<Response<GetCoffeeShopResponse>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var shopDto = await shopRepository
            .QueryAsNoTracking()
            .ProjectToType<ShopDto>(mapper.Config)
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        var result = shopDto == null
            ? Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {request.Id} not found.")
            : Response<GetCoffeeShopResponse>.Success(new GetCoffeeShopResponse(shopDto));

        if (result.IsSuccess)
        {
            await redisService.SetAsync(cacheKey, result);
        }

        return result;
    }
}