using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Handlers.CoffeeShop.Review;

public class AddCoffeeShopReviewRequestHandler(
    IGenericRepository<Domain.Entities.Review> reviewRepository,
    IGenericRepository<Shop> shopsRepository,
    IUnitOfWork unitOfWork,
    IValidationStrategy<AddCoffeeShopReviewRequest> validationStrategy,
    IHybridCache hybridCache,
    IRedisService redisService,
    IOutboxEventPublisher outboxEventPublisher) 
    : IRequestHandler<AddCoffeeShopReviewRequest, Contract.Responses.Response<AddCoffeeShopReviewResponse>>
{
    public async Task<Contract.Responses.Response<AddCoffeeShopReviewResponse>> Handle(AddCoffeeShopReviewRequest request, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Contract.Responses.Response<AddCoffeeShopReviewResponse>.Error(validationResult.ErrorMessage);
        }

        var review = new Shops.Domain.Entities.Review
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
        
        reviewRepository.Add(review);

        // invalidate caches: shop details and city listing
        await InvalidateShopCacheAsync(request.ShopId, cancellationToken);
        
        await outboxEventPublisher.PublishAsync(new ReviewAddedEvent
        {
            UserId = request.UserId,
            ShopId = request.ShopId,
            ReviewId = review.Id,
            CreatedAt = review.ReviewDate
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Contract.Responses.Response<AddCoffeeShopReviewResponse>.Success(new AddCoffeeShopReviewResponse(review.Id));
    }

    private async Task InvalidateShopCacheAsync(Guid shopId, CancellationToken cancellationToken)
    {
        var cityId = await shopsRepository.QueryAsNoTracking()
            .Where(s => s.Id == shopId)
            .Select(s => s.CityId)
            .FirstOrDefaultAsync(cancellationToken);

        await hybridCache.RemoveAsync(CacheKey.Shop.Detail(shopId), cancellationToken);
        if (cityId != Guid.Empty)
        {
            await redisService.RemoveByPatternAsync(CacheKey.Shop.ListByCityPattern(cityId));
        }
    }
}