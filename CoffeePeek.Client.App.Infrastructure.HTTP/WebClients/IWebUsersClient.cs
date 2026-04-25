using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public interface IWebUsersClient
{
    /// <summary>
    /// GET <c>api/Users/exists?email=…</c>. Returns <c>true</c> if the user already exists (go to login).
    /// </summary>
    Task<Result<bool>> EmailIsRegisteredAsync(string email, CancellationToken cancellationToken = default);
}
