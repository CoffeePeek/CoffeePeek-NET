using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Kernel;
using MassTransit;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class ReviewAddedConsumer(IUserRepository userRepository, IUnitOfWork unitOfWork) : IConsumer<ReviewAddedEvent>
{
    public async Task Consume(ConsumeContext<ReviewAddedEvent> context)
    {
        var user = await userRepository.GetById(@context.Message.UserId);

        if (user == null) return;

        user.Statistics.IncrementReviews();

        await unitOfWork.SaveChangesAsync();
    }
}