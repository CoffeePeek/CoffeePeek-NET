using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Services.Headers;

/// <summary>
/// Adds Bearer access token when <see cref="HttpCommand.IsAuthorize"/> is set and token is available.
/// </summary>
/// <remarks>
/// IMPORTANT: This setter modifies <see cref="HttpCommand.Headers"/> per-request, NOT <see cref="HttpClient.DefaultRequestHeaders"/>.
/// HttpClient.DefaultRequestHeaders must NEVER be mutated to ensure presigned upload URLs (which bypass authorization)
/// are not contaminated with auth headers that would cause storage provider rejections.
/// </remarks>
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
