using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Auth;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses.Auth;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public interface IWebAuthenticationClient
{
    Task<Result<TokenResponse>> GetToken(GetTokenRequest request, CancellationToken cancellationToken = default);

    Task<Result<RefreshTokenResponse>> RefreshToken(CancellationToken cancellationToken = default);

    Task<Result> Logout(CancellationToken cancellationToken = default);
}
