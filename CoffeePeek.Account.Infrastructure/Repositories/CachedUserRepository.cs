using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;

namespace CoffeePeek.Auth.Infrastructure.Repositories;

public class CachedUserRepository(
    IUserRepository decorated,
    IRedisService redisService) : IUserRepository
{
    public async Task Update(User user, CancellationToken ct = default)
    {
        await decorated.Update(user, ct);

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

    public Task<User?> GetByEmailConfirmToken(string requestToken, CancellationToken cancellationToken)
        => decorated.GetByEmailConfirmToken(requestToken, cancellationToken);

    Task<User?> IUserRepository.GetById(Guid userId, CancellationToken ct) => decorated.GetById(userId, ct);
}