using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Account.Persistence.Repositories;

public class UserRepository(IGenericRepository<User> userRepository) : IUserRepository
{
    public async Task<User?> GetById(Guid userId, CancellationToken ct = default)
    {
        return await userRepository
            .Query()
            .Include(x => x.RefreshTokens)
            .Include(x => x.Roles)
            .Include(x => x.PhotoMetadata)
            .FirstOrDefaultAsync(c => c.Id == userId, ct);
    }

    public async Task Add(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await userRepository.AddAsync(user, ct);
    }

    public Task Update(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        userRepository.Update(user);
        return Task.CompletedTask;
    }

    public async Task<bool> IsEmailUnique(string email, CancellationToken ct)
    {
        return !await userRepository.AnyAsync(c => c.Credentials.Email == email, ct);
    }

    public Task<User?> GetByEmail(string email, CancellationToken ct)
    {
        return userRepository
            .Query()
            .Include(x => x.Roles)
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(c => c.Credentials.Email == email, ct);
    }

    public Task<User?> GetByProvider(string provider, string providerId, CancellationToken ct)
    {
        return userRepository
            .QueryAsNoTracking()
            .Include(x => x.Credentials)
            .FirstOrDefaultAsync(x => x.Credentials.OAuthProvider == provider, cancellationToken: ct);
    }

    public Task<User?> GetByEmailConfirmToken(string requestToken, CancellationToken cancellationToken)
    {
        return userRepository.FirstOrDefaultAsync(c => c.Credentials.EmailConfirmationToken == requestToken,
            cancellationToken);
    }
}