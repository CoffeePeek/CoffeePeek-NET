using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class ModerationReviewApprovedConsumer(
    IGenericRepository<Review> reviewRepository,
    IUnitOfWork unitOfWork,
    IOutboxEventPublisher outboxEventPublisher,
    ILogger<ModerationReviewApprovedConsumer> logger)
    : IConsumer<ModerationReviewApprovedEvent>
{
    public async Task Consume(ConsumeContext<ModerationReviewApprovedEvent> context)
    {
        var reviewDto = context.Message.Review;
        
        logger.LogInformation("Received ModerationReviewApprovedEvent for UserId: {UserId}, ShopId: {ShopId}", 
            reviewDto.UserId, reviewDto.ShopId);

        var existingReview = await reviewRepository
            .FirstOrDefaultAsync(r => r.ShopId == reviewDto.ShopId && r.UserId == reviewDto.UserId, 
                context.CancellationToken);

        if (existingReview != null)
        {
            logger.LogWarning("Review already exists for UserId: {UserId}, ShopId: {ShopId}. Skipping.", 
                reviewDto.UserId, reviewDto.ShopId);
            return;
        }

        var review = Review.Create(
            reviewDto.ShopId,
            reviewDto.UserId,
            reviewDto.Header,
            reviewDto.Comment,
            reviewDto.RatingCoffee,
            reviewDto.RatingPlace,
            reviewDto.RatingService);

        reviewRepository.Add(review);

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        await outboxEventPublisher.PublishAsync(new ReviewAddedEvent
        {
            UserId = reviewDto.UserId,
            ShopId = reviewDto.ShopId,
            ReviewId = review.Id,
            CreatedAt = review.ReviewDate
        }, context.CancellationToken);

        logger.LogInformation("Review created successfully for UserId: {UserId}, ShopId: {ShopId}, ReviewId: {ReviewId}",
            reviewDto.UserId, reviewDto.ShopId, review.Id);
    }
}
