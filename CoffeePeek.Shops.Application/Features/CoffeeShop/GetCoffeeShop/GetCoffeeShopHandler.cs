using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Persistence;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using Mapster;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

public class GetCoffeeShopHandler(
    ICoffeeShopRepository coffeeShopRepository,
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
                var shop = await coffeeShopRepository.GetByIdAsNoTracking(queryRequest.Id, cancellationToken);

                if (shop == null)
                    return Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {queryRequest.Id} not found.");

                // Reviews list
                var (reviews, avgRating, totalCount) = await reviewRepository.GetReviewsWithStatsByCoffeeShopId(shop.Id, cancellationToken);
                
                var shopDto = shop.Adapt<CoffeeShopDetailsDto>(mapper.Config) with
                {
                    Rating = avgRating,
                    ReviewCount = totalCount,
                    Reviews = reviews.Take(10).Adapt<ReviewDto[]>(mapper.Config)
                };

                return Response<GetCoffeeShopResponse>.Success(new GetCoffeeShopResponse(shopDto));
            },
            cacheKey.DefaultTtl);

        if (response?.Data?.ShopDto != null && queryRequest.UserId.HasValue)
        {
            var userId = queryRequest.UserId.Value;
            var shopId = response.Data.ShopDto.Id;

            var enrichment = await coffeeShopRepository.GetUserShopEnrichmentAsync(userId, shopId, cancellationToken);

            response.Data.ShopDto = response.Data.ShopDto with
            {
                IsFavorite = enrichment.IsFavorite,
                IsVisited = enrichment.IsVisited,
                CanCreateReview = enrichment.ExistingReviewId == null,
                ExistingReviewId = enrichment.ExistingReviewId
            };
        }
        
        return response ?? Response<GetCoffeeShopResponse>.Error($"Coffee shop with ID {queryRequest.Id} not found.");
    }
}
