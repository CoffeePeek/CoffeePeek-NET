using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Constants;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class ReviewAddedHandler(IUserRepository userRepository, IUnitOfWork unitOfWork) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Shops.ReviewAdded)]
    public async Task Handle(ReviewAddedEvent @event, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(@event.UserId, cancellationToken);

        if (user == null) return;

        user.Statistics.IncrementReviews();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}