using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Kernel;

namespace CoffeePeek.Account.Infrastructure.Consumers;

public class ReviewAddedHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
{
    public async Task Handle(ReviewAddedEvent message)
    {
        var user = await userRepository.GetById(message.UserId)
            ?? throw new InvalidOperationException($"User {message.UserId} not found for review-added event");

        user.Statistics.IncrementReviews();
        await unitOfWork.SaveChangesAsync();
    }
}