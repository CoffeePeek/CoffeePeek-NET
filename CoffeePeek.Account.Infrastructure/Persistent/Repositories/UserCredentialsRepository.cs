using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Auth.Infrastructure.Persistent.Repositories;

public class UserCredentialsRepository(IGenericRepository<UserCredential> userRepository) : IUserCredentialsRepository
{
    public Task<UserCredential?> GetByIdWithTokens(Guid userCredentialId, CancellationToken ct)
    {
        return userRepository.QueryAsNoTracking().FirstOrDefaultAsync(x => x.Id == userCredentialId, ct);
    }

    public async Task<bool> UserExists(string email, CancellationToken ct = default)
    {
        return await userRepository.AnyAsync(u => u.Email == email, ct);
    }
}