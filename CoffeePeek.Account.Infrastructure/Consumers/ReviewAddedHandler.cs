using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Kernel;

namespace CoffeePeek.Account.Infrastructure.Consumers;

public class ReviewAddedHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
{
    public async Task Handle(ReviewAddedEvent message)
    {
        var user = await userRepository.GetById(message.UserId);

        if (user == null) return;

        user.Statistics.IncrementReviews();

        await unitOfWork.SaveChangesAsync();
    }
}