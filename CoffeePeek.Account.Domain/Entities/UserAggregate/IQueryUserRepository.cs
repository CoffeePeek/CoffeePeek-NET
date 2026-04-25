namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public interface IQueryUserRepository
{
    void Add(User newUser, CancellationToken ct);
    
    Task<bool> UserExistsByEmail(string requestEmail, CancellationToken cancellationToken);
    Task<User?> GetByProvider(string provider, string providerId, CancellationToken ct);
    Task<User?> GetByEmail(string email, CancellationToken ct);
    Task<bool> IsEmailUnique(string requestEmail, CancellationToken ct);
}