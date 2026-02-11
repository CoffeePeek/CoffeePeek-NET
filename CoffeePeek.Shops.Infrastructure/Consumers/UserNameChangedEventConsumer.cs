using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public static class UserNameChangedHandler
{
    [Transactional]
    public static async Task Handle(
        UserNameChangedEvent @event,
        IReviewRepository reviewRepository,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Updating reviews for user {UserId} with new name: {NewName}",
            @event.UserId, @event.NewUserName);

        var reviews = await reviewRepository.GetByUserId(@event.UserId, ct);

        if (reviews.Length == 0)
        {
            logger.LogInformation("No reviews found for user {UserId}", @event.UserId);
            return;
        }

        foreach (var review in reviews)
        {
            review.UpdateUserName(@event.NewUserName);
        }

        logger.LogInformation("Successfully updated {Count} reviews", reviews.Length);
    }
}