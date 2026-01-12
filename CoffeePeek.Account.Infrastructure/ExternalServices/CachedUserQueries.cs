using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;

namespace CoffeePeek.Auth.Infrastructure.ExternalServices;

public class CachedUserQueries(
    IUserQueries decorated, 
    IRedisService redisService) : IUserQueries
{
    public async Task<UserDto?> GetProfileByIdAsync(Guid userId, CancellationToken ct)
    {
        var cacheKey = CacheKey.User.Profile(userId);
        
        return await redisService.GetAsync(
            cacheKey, 
            () => decorated.GetProfileByIdAsync(userId, ct),
            cacheKey.DefaultTtl);
    }
}