using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Review = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class UserNameChangedHandler(
    IGenericRepository<Review> reviewRepository,
    IUnitOfWork unitOfWork,
    ILogger<UserNameChangedHandler> logger) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Account.UserNameChanged)]
    public async Task Handle(UserNameChangedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received UserNameChangedEvent for UserId: {UserId}, NewUserName: {NewUserName}",
            @event.UserId, @event.NewUserName);

        var reviews = await reviewRepository
            .Query()
            .Where(r => r.UserId == @event.UserId)
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
        {
            logger.LogInformation("No reviews found for UserId: {UserId}", @event.UserId);
            return;
        }

        foreach (var review in reviews)
        {
            review.UpdateUserName(@event.NewUserName);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Updated {Count} reviews with new UserName for UserId: {UserId}",
            reviews.Count, @event.UserId);
    }
}
