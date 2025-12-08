using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.ShopsService.Services.Interfaces;
using MassTransit;
using MediatR;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop.CheckIn;

public class CreateCheckInHandler(
    ShopsDbContext dbContext,
    IValidationStrategy<CreateCheckInRequest> validationStrategy,
    IRedisService redisService,
    IPublishEndpoint publishEndpoint,
    ICacheService cacheService)
    : IRequestHandler<CreateCheckInRequest, Contract.Response.Response<CreateCheckInResponse>>
{
    public async Task<Contract.Response.Response<CreateCheckInResponse>> Handle(CreateCheckInRequest request, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Contract.Response.Response<CreateCheckInResponse>.Error(validationResult.ErrorMessage);
        }

        var checkIn = new Entities.CheckIn.CheckIn
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ShopId = request.ShopId,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };

        Guid? reviewId = null;

        if (request.Review != null)
        {
            var review = new Entities.Review
            {
                Id = Guid.NewGuid(),
                Header = request.Review.Header,
                Comment = request.Review.Comment,
                UserId = request.UserId,
                ShopId = request.ShopId,
                RatingCoffee = request.Review.RatingCoffee,
                RatingPlace = request.Review.RatingPlace,
                RatingService = request.Review.RatingService,
                ReviewDate = DateTime.UtcNow
            };

            dbContext.Reviews.Add(review);
            checkIn.ReviewId = review.Id;
            reviewId = review.Id;

            await InvalidateShopCacheAsync(request.ShopId);
        }

        dbContext.CheckIns.Add(checkIn);
        await dbContext.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new CheckinCreatedEvent
        {
            UserId = request.UserId,
            ShopId = request.ShopId,
            CreatedAt = checkIn.CreatedAt
        }, cancellationToken);

        if (reviewId.HasValue)
        {
            await publishEndpoint.Publish(new ReviewAddedEvent
            {
                UserId = request.UserId,
                ShopId = request.ShopId,
                ReviewId = reviewId.Value,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        return Contract.Response.Response<CreateCheckInResponse>.Success(new CreateCheckInResponse(checkIn.Id, reviewId));
    }

    private async Task InvalidateShopCacheAsync(Guid shopId)
    {
        var cityId = (await cacheService.GetCities()).FirstOrDefault(s => s.Id == shopId)!.Id;

        await redisService.RemoveAsync(CacheKey.Shop.ById(shopId));
        if (cityId != Guid.Empty)
        {
            await redisService.RemoveByPatternAsync(CacheKey.Shop.ByCityPattern(cityId));
        }
    }
}