using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Exceptions;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Common;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CreateCoffeeShopReview;

using Review = Domain.Entities.ReviewAggregate.Review;

public class CreateCoffeeShopReviewHandler(
    IGenericRepository<Review> reviewRepository,
    ICoffeeShopCacheService coffeeShopCacheService,
    IUnitOfWork unitOfWork,
    IValidationStrategy<CreateCoffeeShopReviewCommand> validationStrategy,
    IOutboxEventPublisher outboxEventPublisher)
    : IRequestHandler<CreateCoffeeShopReviewCommand, Response<CreateCoffeeShopReviewResponse>>
{
    public async Task<Response<CreateCoffeeShopReviewResponse>> Handle(
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

        return Response<CreateCoffeeShopReviewResponse>.Success(
            new CreateCoffeeShopReviewResponse(review.Id));
    }
}