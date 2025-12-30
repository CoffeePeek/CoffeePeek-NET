using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Domain.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Handlers.CoffeeShop;

public class GetCoffeeShopHandler(
    IGenericRepository<Shop> shopRepository,
    IMapper mapper,
    IRedisService redisService)
    : IRequestHandler<GetCoffeeShopCommand, Response<GetCoffeeShopResponse>>
{
    public async Task<Response<GetCoffeeShopResponse>> Handle(GetCoffeeShopCommand request,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.CachedShop.ById(request.Id);
        var cached = await redisService.GetAsync<Response<GetCoffeeShopResponse>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var shopDto = await shopRepository
            .QueryAsNoTracking()
            .ProjectToType<ShopDto>(mapper.Config)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

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