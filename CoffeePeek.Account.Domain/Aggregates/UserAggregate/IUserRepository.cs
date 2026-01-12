namespace CoffeePeek.Account.Domain.Aggregates.UserAggregate;

public interface IUserRepository
{
    Task<User?> GetById(Guid userId);
    Task Add(User user, CancellationToken ct = default);
    Task Update(User user, CancellationToken ct = default);
    Task<bool> IsEmailUnique(string email, CancellationToken ct);
    Task<User?> GetByEmail(string email, CancellationToken ct);
    Task<User?> GetByProvider(string provider, string providerId, CancellationToken ct);
}