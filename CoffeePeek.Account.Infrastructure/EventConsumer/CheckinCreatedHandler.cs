using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class CheckinCreatedHandler
{
    [Transactional]
    public async Task Handle(
        CheckinCreatedEvent @event, 
        IUserRepository userRepository, 
        CancellationToken ct)
    {
        var user = await userRepository.GetById(@event.UserId, ct);

        user?.Statistics.IncrementCheckIn();
    }
}