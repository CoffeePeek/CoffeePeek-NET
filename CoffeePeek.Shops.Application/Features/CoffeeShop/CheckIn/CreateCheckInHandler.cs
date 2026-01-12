using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Commands.CoffeeShop;
using CoffeePeek.Shops.Application.Common;
using CoffeePeek.Shops.Application.Services;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CheckIn;

using Review = Domain.Entities.ReviewAggregate.Review;

public class CreateCheckInHandler(
    IGenericRepository<Domain.Entities.CheckIn> checkInRepository,
    IShopCacheService shopCacheService,
    IGenericRepository<Review> reviewsRepository,
    IUnitOfWork unitOfWork,
    IValidationStrategy<CreateCheckInRequest> validationStrategy,
    IOutboxEventPublisher outboxEventPublisher)
    : IRequestHandler<CreateCheckInRequest, Contract.Responses.Response<CreateCheckInResponse>>
{
    public async Task<Contract.Responses.Response<CreateCheckInResponse>> Handle(CreateCheckInRequest request, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Contract.Responses.Response<CreateCheckInResponse>.Error(validationResult.ErrorMessage);
        }

        var checkIn = new Domain.Entities.CheckIn
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
            var reviewCommand = request.Review;
            var review = Review.Create(request.UserId, request.ShopId, reviewCommand.Header, reviewCommand.Comment,
                reviewCommand.RatingCoffee, reviewCommand.RatingPlace, reviewCommand.RatingService);

            reviewsRepository.Add(review);
            checkIn.ReviewId = review.Id;
            reviewId = review.Id;
        }

        checkInRepository.Add(checkIn);
        
        if (request.Review != null)
        {
            await shopCacheService.InvalidateShopCacheAsync(request.ShopId, cancellationToken);
        }
        
        await outboxEventPublisher.PublishAsync(new CheckinCreatedEvent
        {
            UserId = request.UserId,
            ShopId = request.ShopId,
            CreatedAt = checkIn.CreatedAt
        }, cancellationToken);

        if (reviewId.HasValue)
        {
            await outboxEventPublisher.PublishAsync(new ReviewAddedEvent
            {
                UserId = request.UserId,
                ShopId = request.ShopId,
                ReviewId = reviewId.Value,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Contract.Responses.Response<CreateCheckInResponse>.Success(new CreateCheckInResponse(checkIn.Id, reviewId));
    }
}