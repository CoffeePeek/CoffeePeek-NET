using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public static class CheckInCreatedHandler
{
    public static SendReviewToModerationCommand Handle(CheckinCreatedEvent @event)
    {
        var review = @event.ReviewDto;

        return new SendReviewToModerationCommand(
            review.UserId,
            review.Username,
            review.CoffeeShopId,
            review.Header,
            review.Comment,
            review.Rating,
            review.Photos);
    }
}