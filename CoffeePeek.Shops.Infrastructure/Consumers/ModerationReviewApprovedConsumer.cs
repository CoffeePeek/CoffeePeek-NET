using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Review = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class ModerationReviewApprovedHandler(
    IGenericRepository<Review> reviewRepository,
    IUnitOfWork unitOfWork,
    ICapPublisher capPublisher,
    ILogger<ModerationReviewApprovedHandler> logger) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Moderation.ReviewApproved)]
    public async Task Handle(ModerationReviewApprovedEvent @event, CancellationToken cancellationToken)
    {
        var reviewDto = @event.Review;
        
        logger.LogInformation("Received ModerationReviewApprovedEvent for UserId: {UserId}, ShopId: {ShopId}", 
            reviewDto.UserId, reviewDto.ShopId);

        var existingReview = await reviewRepository
            .FirstOrDefaultAsync(r => r.CoffeeShopId == reviewDto.ShopId && r.UserId == reviewDto.UserId, 
                cancellationToken);

        if (existingReview != null)
        {
            logger.LogWarning("Review already exists for UserId: {UserId}, ShopId: {ShopId}. Skipping.", 
                reviewDto.UserId, reviewDto.ShopId);
            return;
        }
        
        var review = Review.Create(
            reviewDto.ShopId,
            reviewDto.UserId,
            reviewDto.UserName,
            reviewDto.Header,
            reviewDto.Comment,
            reviewDto.Rating);

        reviewRepository.Add(review);


        using var trans = unitOfWork.BeginTransactionAsync(cancellationToken);
            
        await capPublisher.PublishAsync(
            name: CapEventNames.Shops.ReviewAdded,
            contentObj: new ReviewAddedEvent(reviewDto.UserId, reviewDto.ShopId, review.Id),
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
            
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        logger.LogInformation(
            "Review created successfully for UserId: {UserId}, ShopId: {ShopId}, ReviewId: {ReviewId}",
            reviewDto.UserId, reviewDto.ShopId, review.Id);
    }
}
