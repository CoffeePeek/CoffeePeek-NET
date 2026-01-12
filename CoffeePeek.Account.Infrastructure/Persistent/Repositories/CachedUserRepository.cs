using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;

namespace CoffeePeek.Auth.Infrastructure.Persistent.Repositories;

public class CachedUserRepository(
    IUserRepository decorated,
    IRedisService redisService) : IUserRepository
{
    public async Task Update(User user)
    {
        await decorated.Update(user);

        await redisService.RemoveAsync(CacheKey.User.Profile(user.Id));
        await redisService.RemoveAsync(CacheKey.User.Entity(user.Id));
    }

    public Task Add(User user, CancellationToken ct = default) => decorated.Add(user, ct);

    public Task<bool> IsEmailUnique(string email, CancellationToken ct) => decorated.IsEmailUnique(email, ct);

    public Task<User?> GetByEmail(string email, CancellationToken ct) =>
        decorated.GetByEmail(email, ct);

    public Task<User?>
        GetByProvider(string provider, string providerId, CancellationToken ct) =>
        decorated.GetByProvider(provider, providerId, ct);

    Task<User?> IUserRepository.GetById(Guid userId) => decorated.GetById(userId);
}