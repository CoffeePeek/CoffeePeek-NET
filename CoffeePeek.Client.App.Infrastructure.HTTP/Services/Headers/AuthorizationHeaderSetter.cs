using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Services.Headers;

/// <summary>
/// Adds Bearer access token when <see cref="HttpCommand.IsAuthorize"/> is set and token is available.
/// </summary>
public sealed class AuthorizationHeaderSetter(IClientSession clientSession) : IHeaderSetter
{
    public void SetHeader(HttpCommand command)
    {
        if (!command.IsAuthorize)
            return;

        var token = clientSession.AccessToken;
        if (string.IsNullOrEmpty(token))
            return;

        if (command.Headers.ContainsKey("Authorization"))
            return;

        command.Headers["Authorization"] = "Bearer " + token;
    }
}
