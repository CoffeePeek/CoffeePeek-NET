using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Persistence;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

public class GetCoffeeShopHandler(
    IGenericRepository<Domain.Entities.CoffeeShopAggregate.CoffeeShop> shopRepository,
    IUserFavoriteRepository favoriteRepository,
    IUserCheckInRepository checkInRepository,
    IMapper mapper,
    IRedisService redisService)
    : IRequestHandler<GetCoffeeShopQuery, Response<GetCoffeeShopResponse>>
{
    public async Task<Response<GetCoffeeShopResponse>> Handle(GetCoffeeShopQuery queryRequest,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.Shop.Detail(queryRequest.Id);
        
        var response = await redisService.GetAsync(
            cacheKey,
            async () =>
            {
                var shop = await shopRepository
                    .QueryAsNoTracking()
                    .Include(x => x.ShopPhotos)
                    .Include(x => x.Reviews)
                    .Include(x => x.Equipments)
                    .Include(x => x.CoffeeBeans)
                    .Include(x => x.Roasters)
                    .Include(x => x.BrewMethods)
                    .Include(x => x.Contact)
                    .Include(x => x.Location)
                    .Include(x => x.Schedules)
                    .FirstOrDefaultAsync(x => x.Id == queryRequest.Id, cancellationToken);

                var shopDto = shop?.Adapt<CoffeeShopDetailsDto>(mapper.Config);

                return shopDto == null
                    ? Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {queryRequest.Id} not found.")
                    : Response<GetCoffeeShopResponse>.Success(new GetCoffeeShopResponse(shopDto));
            },
            cacheKey.DefaultTtl);

        if (queryRequest.UserId.HasValue)
        {
            var userId = queryRequest.UserId.Value;
        
            var enrichedCoffeeShop = response?.Data.ShopDto with
            {
                IsFavorite =
                await favoriteRepository.Exists(userId, response.Data.ShopDto.Id, cancellationToken),
                IsVisited = await checkInRepository.Exists(userId, response.Data.ShopDto.Id, cancellationToken)
            };
            
            response.Data.ShopDto = enrichedCoffeeShop;
        }
        
        return response ?? Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {response.Data.ShopDto.Id} not found.");
    }
}