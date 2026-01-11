using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;

namespace CoffeePeek.Auth.Infrastructure.ExternalServices;

public class CachedUserQueries(
    IUserQueries decorated, 
    IHybridCache hybridCache) : IUserQueries
{
    public async Task<UserDto?> GetProfileByIdAsync(Guid userId, CancellationToken ct)
    {
        var cacheKey = CacheKey.User.Profile(userId);
        
        return await hybridCache.GetOrSetAsync(
            cacheKey, 
            () => decorated.GetProfileByIdAsync(userId, ct),
            distributedTtl: cacheKey.DefaultTtl,
            memoryTtl: TimeSpan.FromMinutes(5),
            ct: ct);
    }
}