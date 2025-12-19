using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.User.Domain.Repositories;
using MassTransit;

namespace CoffeePeek.User.Infrastructure.EventConsumer;

public class UserRegisteredEventConsumer(
    IUserRepository userRepository,
    IRedisService redisService,
    IUnitOfWork unitOfWork) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;

        if (await userRepository.GetByIdAsync(@event.UserId) != null) return;

        var user = new User.Domain.Entities.User
        {
            Id = @event.UserId,
            Email = @event.Email,
            Username = @event.UserName
        };

        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        await redisService.SetAsync(CacheKey.User.ById(user.Id), user);
    }
}