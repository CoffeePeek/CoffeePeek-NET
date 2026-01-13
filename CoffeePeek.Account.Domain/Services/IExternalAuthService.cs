using CoffeePeek.Account.Domain.Entities.UserAggregate;

namespace CoffeePeek.Account.Domain.Services;

public interface IExternalAuthService
{
    Task<User> GetOrCreate(string email, string provider, string providerId, CancellationToken ct);
}