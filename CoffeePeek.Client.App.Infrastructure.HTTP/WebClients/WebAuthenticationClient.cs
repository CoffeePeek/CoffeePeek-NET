using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Auth;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses.Auth;
using CoffeePeek.Client.App.Infrastructure.HTTP.Services;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients.Interfaces;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public sealed class WebAuthenticationClient(IHttpCommandExecutor commandExecutor, ITokenRefresher tokenRefresher)
    : WebClientBase(commandExecutor), IWebAuthenticationClient
{
    public Task<Result<TokenResponse>> GetToken(GetTokenRequest request, CancellationToken cancellationToken = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Post)
            .WithEndpoint("api/Tokens")
            .WithBody(request);

        return Execute<TokenResponse>(command, cancellationToken);
    }

    public Task<Result<RefreshTokenResponse>> RefreshToken(CancellationToken cancellationToken = default) =>
        tokenRefresher.RefreshAsync(cancellationToken);

    public Task<Result> Logout(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
