using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Entities;

namespace CoffeePeek.Account.Domain.Repositories;

public interface IUserCredentialsRepository
{
    Task<UserCredential?> GetByIdWithTokens(Guid userCredentialId, CancellationToken ct);
    Task<bool> UserExists(string requestEmail, CancellationToken cancellationToken);
}