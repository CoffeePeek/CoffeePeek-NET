using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Handlers.CoffeeShop.Review;

public class UpdateCoffeeShopReviewRequestHandler(
    IGenericRepository<Domain.Entities.Review> reviewRepository,
    IGenericRepository<Shop> shopsRepository,
    IUnitOfWork unitOfWork,
    IValidationStrategy<UpdateCoffeeShopReviewRequest> validationStrategy,
    IRedisService redisService)
    : IRequestHandler<UpdateCoffeeShopReviewRequest, Response<UpdateCoffeeShopReviewResponse>>
{
    public async Task<Response<UpdateCoffeeShopReviewResponse>> Handle(UpdateCoffeeShopReviewRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error(validationResult.ErrorMessage);
        }

        var review = await reviewRepository
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error("Review not found");
        }

        if (review.UserId != request.UserId)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error("You are not authorized to update this review");
        }

        review.Header = request.Header;
        review.Comment = request.Comment;
        review.RatingCoffee = request.RatingCoffee;
        review.RatingService = request.RatingService;
        review.RatingPlace = request.RatingPlace;

        reviewRepository.Update(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await InvalidateShopCacheAsync(review.ShopId, cancellationToken);

        return Response<UpdateCoffeeShopReviewResponse>.Success(new UpdateCoffeeShopReviewResponse(review.Id));
    }

    private async Task InvalidateShopCacheAsync(Guid shopId, CancellationToken cancellationToken)
    {
        var cityId = await shopsRepository.QueryAsNoTracking()
            .Where(s => s.Id == shopId)
            .Select(s => s.CityId)
            .FirstOrDefaultAsync(cancellationToken);

        await redisService.RemoveAsync(CacheKey.CachedShop.ById(shopId));
        if (cityId != Guid.Empty)
        {
            await redisService.RemoveByPatternAsync(CacheKey.CachedShop.ByCityPattern(cityId));
        }
    }
}