using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public static class ModerationReviewApprovedHandler
{
    [Transactional]
    public static async Task<ReviewAddedEvent?> Handle(
        ModerationReviewApprovedEvent @event,
        IReviewRepository reviewRepository,
        ILogger logger,
        CancellationToken ct)
    {
        var reviewDto = @event.Review;
        
        logger.LogInformation("Processing approved review for User: {UserId}, Shop: {ShopId}", 
            reviewDto.UserId, reviewDto.ShopId);

        var exists = await reviewRepository.AnyAsync(reviewDto.ShopId, reviewDto.UserId, ct);

        if (exists)
        {
            logger.LogWarning("Review already exists. Skipping creation.");
            return null;
        }
        
        var review = Review.Create(
            reviewDto.ShopId,
            reviewDto.UserId,
            reviewDto.UserName,
            reviewDto.Header,
            reviewDto.Comment,
            reviewDto.Rating.Place, reviewDto.Rating.Service, reviewDto.Rating.Coffee);

        reviewRepository.Add(review);

        logger.LogInformation("Review created. ID: {ReviewId}", review.Id);

        return new ReviewAddedEvent(reviewDto.UserId, reviewDto.ShopId, review.Id);
    }
}
