using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.UserService.Repositories;
using MapsterMapper;
using MassTransit;

namespace CoffeePeek.UserService.EventConsumer;

public class UserLoggedInEventConsumer(
    IUserRepository userRepository,
    IMapper mapper,
    IRedisService redisService) 
    : IConsumer<UserLoggedInEvent>
{
    public async Task Consume(ConsumeContext<UserLoggedInEvent> context)
    {
        var @event = context.Message;
        
        var userFromCache = await redisService.GetAsync<UserDto>(CacheKey.User.Profile(@event.UserId));

        if (userFromCache != null)
        {
            return;
        }
        
        var user = await userRepository.GetByIdAsync(@event.UserId);
        if (user == null) return;
        
        var userDto = mapper.Map<UserDto>(user);
        
        await redisService.SetAsync(CacheKey.User.Profile(@event.UserId), userDto);
    }
}