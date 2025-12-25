using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Account.Domain.Services;

public class UserRepository(IGenericRepository<User> userRepository) : IUserRepository
{
    public async Task<User?> GetById(Guid userId)
    {
        return await userRepository.GetByIdAsync(userId);
    }

    public async Task Add(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await userRepository.AddAsync(user, ct);
    }

    public Task Update(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        userRepository.Update(user);
        return Task.CompletedTask;
    }

    public async Task<bool> IsEmailUnique(string email, CancellationToken ct)
    {
        return !await userRepository.AnyAsync(c => c.Email == email, ct);
    }

    public Task<User?> GetByEmail(string email, CancellationToken ct)
    {
        return userRepository.FirstOrDefaultAsNoTrackingAsync(c => c.Email == email, ct);
    }

    public Task<User?> GetByProvider(string provider, string providerId, CancellationToken ct)
    {
        return userRepository
            .QueryAsNoTracking()
            .Include(x => x.UserCredential)
            .FirstOrDefaultAsync(x => x.UserCredential.OAuthProvider == provider, cancellationToken: ct);
    }
}