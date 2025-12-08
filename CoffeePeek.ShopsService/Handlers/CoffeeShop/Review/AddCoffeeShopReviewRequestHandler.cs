using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop.Review;

public class AddCoffeeShopReviewRequestHandler(
    ShopsDbContext dbContext,
    IValidationStrategy<AddCoffeeShopReviewRequest> validationStrategy,
    IRedisService redisService) 
    : IRequestHandler<AddCoffeeShopReviewRequest, Response<AddCoffeeShopReviewResponse>>
{
    public async Task<Response<AddCoffeeShopReviewResponse>> Handle(AddCoffeeShopReviewRequest request, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Response<AddCoffeeShopReviewResponse>.Error(validationResult.ErrorMessage);
        }

        var review = new Entities.Review
        {
            Header = request.Header,
            Comment = request.Comment,
            UserId = request.UserId,
            ShopId = request.ShopId,
            RatingCoffee = request.RatingCoffee,
            RatingPlace = request.RatingPlace,
            RatingService = request.RatingService,
        };
        
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);

        // invalidate caches: shop details and city listing
        await InvalidateShopCacheAsync(request.ShopId, cancellationToken);

        return Response<AddCoffeeShopReviewResponse>.Success(new AddCoffeeShopReviewResponse(review.Id));
    }

    private async Task InvalidateShopCacheAsync(Guid shopId, CancellationToken cancellationToken)
    {
        var cityId = await dbContext.Shops
            .AsNoTracking()
            .Where(s => s.Id == shopId)
            .Select(s => s.CityId)
            .FirstOrDefaultAsync(cancellationToken);

        await redisService.RemoveAsync(CacheKey.Shop.ById(shopId));
        if (cityId != Guid.Empty)
        {
            await redisService.RemoveByPatternAsync(CacheKey.Shop.ByCityPattern(cityId));
        }
    }
}