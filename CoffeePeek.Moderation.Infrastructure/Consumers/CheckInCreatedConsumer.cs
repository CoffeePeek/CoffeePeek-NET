using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;
using MediatR;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public class CheckInCreatedConsumer(IMediator mediator) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Shops.CheckinCreated)]
    public async Task Handle(CheckinCreatedEvent @event, CancellationToken cancellationToken)
    {
        var review = @event.ReviewDto;
        var command = new SendReviewToModerationCommand(
            review.UserId, 
            review.Username, 
            review.CoffeeShopId,
            review.Header, 
            review.Comment, 
            review.Rating, 
            review.Photos);
        
        await mediator.Publish(command, cancellationToken);
    }
}