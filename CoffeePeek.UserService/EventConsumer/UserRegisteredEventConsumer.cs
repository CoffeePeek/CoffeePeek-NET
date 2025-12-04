using CoffeePeek.Contract.Events;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.UserService.Models;
using CoffeePeek.UserService.Repositories;
using MassTransit;

namespace CoffeePeek.UserService.EventConsumer;

public class UserRegisteredEventConsumer(
    IUserRepository userRepository,
    IRedisService redisService,
    IUnitOfWork unitOfWork) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;

        if (await userRepository.GetByIdAsync(@event.UserId) != null) return;

        var user = new User
        {
            Id = @event.UserId,
            Email = @event.Email,
            Username = @event.UserName
        };

        await userRepository.AddAsync(user);
        await redisService.SetAsync($"{nameof(User)}{user.Id}", user);

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}