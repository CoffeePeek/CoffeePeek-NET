using CoffeePeek.Account.Domain.Aggregates.UserAggregate;

namespace CoffeePeek.Account.Domain.Repositories;

public interface IUserCredentialsRepository
{
    Task<UserCredential?> GetById(Guid userCredentialId, CancellationToken ct = default);
    Task<UserCredential?> GetByEmailConfirmToken(string emailConfirmationToken, CancellationToken ct = default);
    Task<bool> UserExists(string requestEmail, CancellationToken ct = default);
}