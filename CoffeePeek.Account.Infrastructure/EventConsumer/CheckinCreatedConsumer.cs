using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Kernel;
using MassTransit;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class CheckinCreatedConsumer(IUserRepository userRepository, IUnitOfWork unitOfWork) : IConsumer<CheckinCreatedEvent>
{
    public async Task Consume(ConsumeContext<CheckinCreatedEvent> context)
    {
        var user = await userRepository.GetById(context.Message.UserId);

        user?.Statistics.IncrementCheckIn();

        await unitOfWork.SaveChangesAsync();
    }
}