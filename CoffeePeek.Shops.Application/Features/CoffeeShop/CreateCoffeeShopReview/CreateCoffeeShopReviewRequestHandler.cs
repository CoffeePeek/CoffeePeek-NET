using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Common;
using CoffeePeek.Shops.Application.Services;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CreateCoffeeShopReview;

using Review = Domain.Entities.ReviewAggregate.Review;

public class CreateCoffeeShopReviewRequestHandler(
    IGenericRepository<Review> reviewRepository,
    ICoffeeShopCacheService coffeeShopCacheService,
    IUnitOfWork unitOfWork,
    IValidationStrategy<CreateCoffeeShopReviewCommand> validationStrategy,
    IOutboxEventPublisher outboxEventPublisher)
    : IRequestHandler<CreateCoffeeShopReviewCommand, Contract.Responses.Response<CreateCoffeeShopReviewResponse>>
{
    public async Task<Contract.Responses.Response<CreateCoffeeShopReviewResponse>> Handle(
        CreateCoffeeShopReviewCommand command, CancellationToken ct)
    {
        var validationResult = validationStrategy.Validate(command);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.ErrorMessage);
        }

        var reviewExists = await reviewRepository
                .AnyAsync(r => r.ShopId == command.ShopId && r.UserId == command.UserId, ct);

        if (reviewExists)
        {
            throw new ValidationException("Review already exists");
        }
        
        var review = Review.Create(command.ShopId, command.UserId, command.Header, command.Comment,
            command.RatingCoffee, command.RatingPlace, command.RatingService);

        reviewRepository.Add(review);
        
        await outboxEventPublisher.PublishAsync(new ReviewAddedEvent
        {
            UserId = command.UserId,
            ShopId = command.ShopId,
            ReviewId = review.Id,
            CreatedAt = review.ReviewDate
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);
        
        await coffeeShopCacheService.InvalidateShopCacheAsync(command.ShopId, ct);

        return Contract.Responses.Response<CreateCoffeeShopReviewResponse>.Success(
            new CreateCoffeeShopReviewResponse(review.Id));
    }
}