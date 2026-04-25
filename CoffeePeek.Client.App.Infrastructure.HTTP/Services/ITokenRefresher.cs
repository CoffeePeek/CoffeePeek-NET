using CoffeePeek.Client.App.Infrastructure.HTTP.Responses.Auth;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Services;

/// <summary>
/// Refreshes access token using the same <see cref="HttpClient"/> (cookies). Does not use <see cref="Pipeline.Abstract.HttpPipeline"/> to avoid recursion with 401 handling.
/// </summary>
public interface ITokenRefresher
{
    Task<Result<RefreshTokenResponse>> RefreshAsync(CancellationToken cancellationToken = default);
}
