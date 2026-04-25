using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

public static class GetCoffeeShopHandler
{
    public static async Task<Response<GetCoffeeShopResponse>> Handle(
        GetCoffeeShopQuery query,
        ICoffeeShopQueries shopQueries,
        IUserFavoriteRepository favoriteRepository,
        IQueryCheckInRepository checkInRepository,
        IQueryReviewRepository reviewRepository,
        ICacheService redisService,
        IMapper mapper,
        CancellationToken ct)
    {
        var cacheKey = CacheKey.Shop.Detail(query.Id);

        var shopDto = await redisService.GetAsync(cacheKey, async () =>
        {
            var dto = await shopQueries.GetDetailsById(query.Id, ct);
            if (dto == null) return null;

            var (avgRating, count) = await reviewRepository.GetReviewStatsByCoffeeShopId(query.Id, ct);
            var (reviews, _) = await reviewRepository.GetByCoffeeShopId(query.Id, 1, 10, ct);

            return dto with { 
                Rating = avgRating, 
                ReviewCount = count, 
                Reviews = mapper.Map<ReviewDto[]>(reviews)
            };
        }, cacheKey.DefaultTtl);

        if (shopDto == null)
            return Response<GetCoffeeShopResponse>.Error("Shop not found");

        if (query.UserId.HasValue)
        {
            var userId = query.UserId.Value;
            shopDto = shopDto with
            {
                IsFavorite = await favoriteRepository.Exists(userId, query.Id, ct),
                IsVisited = await checkInRepository.Exists(userId, query.Id, ct)
            };
        }

        return Response<GetCoffeeShopResponse>.Success(new GetCoffeeShopResponse(shopDto));
    }
}