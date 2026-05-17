using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class UserNameChangedHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
{
    public async Task Handle(UserNameChangedEvent message, CancellationToken cancellationToken = default)
    {
        var reviews = await reviewRepository.GetByUserId(message.UserId, cancellationToken);

        if (reviews.Length == 0)
        {
            return;
        }

        foreach (var review in reviews)
        {
            review.UpdateUserName(message.NewUserName);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}