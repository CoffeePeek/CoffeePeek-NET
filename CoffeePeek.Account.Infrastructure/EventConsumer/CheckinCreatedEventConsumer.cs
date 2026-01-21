using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;

namespace CoffeePeek.Auth.Infrastructure.EventConsumer;

//TODO move business logic from consumer
public class CheckinCreatedHandler(IUserRepository userRepository, IUnitOfWork unitOfWork) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Shops.CheckinCreated)]
    public async Task Handle(CheckinCreatedEvent @event, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(@event.UserId);
        if (user == null) return;

        user.Statistics.IncrementCheckIn();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}