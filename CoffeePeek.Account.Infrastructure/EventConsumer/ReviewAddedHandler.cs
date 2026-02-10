using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class ReviewAddedHandler
{
    [Transactional]
    public async Task Handle(
        ReviewAddedEvent @event, 
        IUserRepository userRepository, 
        CancellationToken ct)
    {
        var user = await userRepository.GetById(@event.UserId, ct);

        if (user == null) return;

        user.Statistics.IncrementReviews();
    }
}