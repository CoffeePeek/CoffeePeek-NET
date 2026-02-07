using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class CheckinCreatedHandler(IUserRepository userRepository, IUnitOfWork unitOfWork) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Shops.CheckinCreated)]
    public async Task Handle(CheckinCreatedEvent @event, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(@event.UserId, cancellationToken);
        if (user == null) 
            return;

        user.Statistics.IncrementCheckIn();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}