using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MassTransit;

namespace CoffeePeek.Auth.Infrastructure.EventConsumer;

public class ReviewAddedEventConsumer(IUserRepository userRepository, IUnitOfWork unitOfWork) : IConsumer<ReviewAddedEvent>
{
    public async Task Consume(ConsumeContext<ReviewAddedEvent> context)
    {
        var user = await userRepository.GetById(context.Message.UserId);

        if (user == null) return;

        user.Statistics.IncrementReviews();

        await unitOfWork.SaveChangesAsync();
    }
}