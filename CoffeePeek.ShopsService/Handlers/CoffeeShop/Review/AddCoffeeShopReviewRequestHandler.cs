using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop.Review;

public class AddCoffeeShopReviewRequestHandler(
    ShopsDbContext dbContext,
    IValidationStrategy<AddCoffeeShopReviewRequest> validationStrategy,
    IRedisService redisService,
    IPublishEndpoint publishEndpoint) 
    : IRequestHandler<AddCoffeeShopReviewRequest, Contract.Responses.Response<AddCoffeeShopReviewResponse>>
{
    public async Task<Contract.Responses.Response<AddCoffeeShopReviewResponse>> Handle(AddCoffeeShopReviewRequest request, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Contract.Responses.Response<AddCoffeeShopReviewResponse>.Error(validationResult.ErrorMessage);
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
            ReviewDate = DateTime.UtcNow
        };
        
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);

        // invalidate caches: shop details and city listing
        await InvalidateShopCacheAsync(request.ShopId, cancellationToken);
        
        await publishEndpoint.Publish(new ReviewAddedEvent
        {
            UserId = request.UserId,
            ShopId = request.ShopId,
            ReviewId = review.Id,
            CreatedAt = review.ReviewDate
        }, cancellationToken);

        return Contract.Responses.Response<AddCoffeeShopReviewResponse>.Success(new AddCoffeeShopReviewResponse(review.Id));
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