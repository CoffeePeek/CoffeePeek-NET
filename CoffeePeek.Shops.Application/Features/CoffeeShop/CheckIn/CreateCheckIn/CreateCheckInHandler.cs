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
    IValidationStrategy<CreateCheckInCommand> validationStrategy,
    IOutboxEventPublisher outboxEventPublisher)
    : IRequestHandler<CreateCheckInCommand, Response<CreateCheckInResponse>>
{
    public async Task<Response<CreateCheckInResponse>> Handle(CreateCheckInCommand command, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(command);
        if (!validationResult.IsValid)
        {
            return Response<CreateCheckInResponse>.Error(validationResult.ErrorMessage);
        }

        var checkIn = Domain.Entities.CheckIn.Create(command.UserId, command.ShopId, command.Note);

        Guid? reviewId = null;
        var hasReview = false;

        if (command.Review != null)
        {
            var reviewCommand = command.Review;
            var review = Review.Create(command.ShopId, command.UserId, command.UserName, reviewCommand.Header, reviewCommand.Comment,
                reviewCommand.RatingCoffee, reviewCommand.RatingPlace, reviewCommand.RatingService);

            reviewsRepository.Add(review);
            checkIn.LinkReview(review.Id);
            reviewId = review.Id;
            hasReview = true;
        }

        checkInRepository.Add(checkIn);
        
        await userVisitService.RegisterVisitAsync(
            command.UserId,
            command.ShopId,
            checkIn.CreatedAtUtc,
            hasReview,
            cancellationToken);
        
        if (command.Review != null)
        {
            await coffeeShopCacheService.InvalidateShopCacheAsync(command.ShopId, cancellationToken);
        }
        
        await outboxEventPublisher.PublishAsync(new CheckinCreatedEvent
        {
            UserId = command.UserId,
            ShopId = command.ShopId,
            CreatedAt = checkIn.CreatedAtUtc
        }, cancellationToken);

        if (reviewId.HasValue)
        {
            await outboxEventPublisher.PublishAsync(new ReviewAddedEvent
            {
                UserId = command.UserId,
                ShopId = command.ShopId,
                ReviewId = reviewId.Value,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response<CreateCheckInResponse>.Success(new CreateCheckInResponse(checkIn.Id, reviewId));
    }
}