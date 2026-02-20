using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using MassTransit;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class UserNameChangedHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork) : IConsumer<UserNameChangedEvent>
{
    public async Task Consume(ConsumeContext<UserNameChangedEvent> context)
    {
        var @event = context.Message;
        
        var reviews = await reviewRepository.GetByUserId(@event.UserId, CancellationToken.None);

        if (reviews.Length == 0)
        {
            return;
        }

        foreach (var review in reviews)
        {
            review.UpdateUserName(@event.NewUserName);
        }

        await unitOfWork.SaveChangesAsync();
    }
}