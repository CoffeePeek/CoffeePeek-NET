namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public interface IUserRepository
{
    Task<User?> GetById(Guid userId, CancellationToken ct = default);
    Task Add(User user, CancellationToken ct = default);
    Task Update(User user, CancellationToken ct = default);
    Task<bool> IsEmailUnique(string email, CancellationToken ct);
    Task<User?> GetByEmail(string email, CancellationToken ct);
    Task<User?> GetByProvider(string provider, string providerId, CancellationToken ct);
    Task<bool> UserExistsByEmail(string requestEmail, CancellationToken cancellationToken);
    Task<User?> GetByEmailConfirmToken(string requestToken, CancellationToken cancellationToken);
}