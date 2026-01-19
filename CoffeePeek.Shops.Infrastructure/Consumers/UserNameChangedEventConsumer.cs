using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class UserNameChangedEventConsumer(
    IGenericRepository<Review> reviewRepository,
    IUnitOfWork unitOfWork,
    ILogger<UserNameChangedEventConsumer> logger)
    : IConsumer<UserNameChangedEvent>
{
    public async Task Consume(ConsumeContext<UserNameChangedEvent> context)
    {
        var @event = context.Message;
        
        logger.LogInformation("Received UserNameChangedEvent for UserId: {UserId}, NewUserName: {NewUserName}",
            @event.UserId, @event.NewUserName);

        var reviews = await reviewRepository
            .Query()
            .Where(r => r.UserId == @event.UserId)
            .ToListAsync(context.CancellationToken);

        if (reviews.Count == 0)
        {
            logger.LogInformation("No reviews found for UserId: {UserId}", @event.UserId);
            return;
        }

        foreach (var review in reviews)
        {
            review.UpdateUserName(@event.NewUserName);
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("Updated {Count} reviews with new UserName for UserId: {UserId}",
            reviews.Count, @event.UserId);
    }
}
