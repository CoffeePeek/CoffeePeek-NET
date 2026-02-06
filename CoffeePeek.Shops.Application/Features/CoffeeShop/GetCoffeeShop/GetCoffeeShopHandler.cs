using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Persistence;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

public class GetCoffeeShopHandler(
    IGenericRepository<Domain.Aggregates.CoffeeShopAggregate.CoffeeShop> shopRepository,
    IUserFavoriteRepository favoriteRepository,
    IUserCheckInRepository checkInRepository,
    IReviewRepository reviewRepository,
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
                    .Include(x => x.Equipments)
                    .Include(x => x.CoffeeBeans)
                    .Include(x => x.Roasters)
                    .Include(x => x.BrewMethods)
                    .Include(x => x.Schedules)
                    .FirstOrDefaultAsync(x => x.Id == queryRequest.Id, cancellationToken);

                if (shop == null)
                    return Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {queryRequest.Id} not found.");

                // Get review statistics and reviews through the repository
                var (averageRating, reviewCount) = await reviewRepository.GetReviewStatsByCoffeeShopIdAsync(queryRequest.Id, cancellationToken);
                var (reviews, _) = await reviewRepository.GetByCoffeeShopIdAsync(queryRequest.Id, page: 1, pageSize: 10, cancellationToken);
                
                var shopDto = shop.Adapt<CoffeeShopDetailsDto>(mapper.Config) with
                {
                    Rating = averageRating,
                    ReviewCount = reviewCount,
                    Reviews = reviews.Adapt<ReviewDto[]>(mapper.Config)
                };

                return Response<GetCoffeeShopResponse>.Success(new GetCoffeeShopResponse(shopDto));
            },
            cacheKey.DefaultTtl);

        if (response?.Data?.ShopDto != null && queryRequest.UserId.HasValue)
        {
            var userId = queryRequest.UserId.Value;
        
            var enrichedCoffeeShop = response.Data.ShopDto with
            {
                IsFavorite = await favoriteRepository.Exists(userId, response.Data.ShopDto.Id, cancellationToken),
                IsVisited = await checkInRepository.Exists(userId, response.Data.ShopDto.Id, cancellationToken)
            };
            
            response.Data.ShopDto = enrichedCoffeeShop;
        }
        
        return response ?? Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {queryRequest.Id} not found.");
    }
}