using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Kernel;

namespace CoffeePeek.Account.Infrastructure.Consumers;

public class CheckinCreatedHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
{
    public async Task Handle(CheckinCreatedEvent message, CancellationToken ct)
    {
        var user = await userRepository.GetById(message.UserId, ct)
            ?? throw new InvalidOperationException($"User {message.UserId} not found for check-in event");

        user.Statistics.IncrementCheckIn();
        await unitOfWork.SaveChangesAsync(ct);
    }
}