using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Common;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CheckIn.CreateCheckIn;

using Review = Domain.Entities.ReviewAggregate.Review;

public class CreateCheckInHandler(
    IGenericRepository<Domain.Entities.CheckIn> checkInRepository,
    ICoffeeShopCacheService coffeeShopCacheService,
    IGenericRepository<Review> reviewsRepository,
    IUserVisitService userVisitService,
    IUnitOfWork unitOfWork,
    IValidationStrategy<CreateCheckInRequest> validationStrategy,
    IOutboxEventPublisher outboxEventPublisher)
    : IRequestHandler<CreateCheckInRequest, Response<CreateCheckInResponse>>
{
    public async Task<Response<CreateCheckInResponse>> Handle(CreateCheckInRequest request, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Response<CreateCheckInResponse>.Error(validationResult.ErrorMessage);
        }

        var checkIn = Domain.Entities.CheckIn.Create(request.UserId, request.ShopId, request.Note);

        Guid? reviewId = null;
        var hasReview = false;

        if (request.Review != null)
        {
            var reviewCommand = request.Review;
            var review = Review.Create(request.UserId, request.ShopId, reviewCommand.Header, reviewCommand.Comment,
                reviewCommand.RatingCoffee, reviewCommand.RatingPlace, reviewCommand.RatingService);

            reviewsRepository.Add(review);
            checkIn.LinkReview(review.Id);
            reviewId = review.Id;
            hasReview = true;
        }

        checkInRepository.Add(checkIn);
        
        await userVisitService.RegisterVisitAsync(
            request.UserId,
            request.ShopId,
            checkIn.CreatedAtUtc,
            hasReview,
            cancellationToken);
        
        if (request.Review != null)
        {
            await coffeeShopCacheService.InvalidateShopCacheAsync(request.ShopId, cancellationToken);
        }
        
        await outboxEventPublisher.PublishAsync(new CheckinCreatedEvent
        {
            UserId = request.UserId,
            ShopId = request.ShopId,
            CreatedAt = checkIn.CreatedAtUtc
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

        return Response<CreateCheckInResponse>.Success(new CreateCheckInResponse(checkIn.Id, reviewId));
    }
}