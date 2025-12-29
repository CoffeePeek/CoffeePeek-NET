using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Auth.Infrastructure.Persistent.Repositories;

public class UserCredentialsRepository(IGenericRepository<UserCredential> userRepository) : IUserCredentialsRepository
{
    public Task<UserCredential?> GetById(Guid userCredentialId, CancellationToken ct = default)
    {
        return userRepository.FirstOrDefaultAsync(x => x.Id == userCredentialId, ct);
    }

    public Task<UserCredential?> GetByEmailConfirmToken(string emailConfirmationToken, CancellationToken ct = default)
    {
        return userRepository.FirstOrDefaultAsync(x => x.EmailConfirmationToken == emailConfirmationToken, ct);
    }

    public async Task<bool> UserExists(string email, CancellationToken ct = default)
    {
        return await userRepository.AnyAsync(u => u.Email == email, ct);
    }
}