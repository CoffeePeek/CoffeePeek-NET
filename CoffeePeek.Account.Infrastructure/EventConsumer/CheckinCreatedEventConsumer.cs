using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MassTransit;

namespace CoffeePeek.Auth.Infrastructure.EventConsumer;

//TODO move business logic from consumer
public class CheckinCreatedEventConsumer(IUserRepository userRepository, IUnitOfWork unitOfWork) : IConsumer<CheckinCreatedEvent>
{
    public async Task Consume(ConsumeContext<CheckinCreatedEvent> context)
    {
        var @event = context.Message;

        var user = await userRepository.GetById(@event.UserId);
        if (user == null) return;

        user.Statistics.IncrementCheckIn();

        await unitOfWork.SaveChangesAsync();
    }
}