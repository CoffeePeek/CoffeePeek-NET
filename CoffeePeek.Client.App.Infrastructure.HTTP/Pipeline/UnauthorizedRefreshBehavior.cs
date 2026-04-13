using System.Net;
using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Services;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline;

/// <summary>
/// On HTTP 401 for an authorized command, refreshes the access token once and retries the inner pipeline.
/// </summary>
public sealed class UnauthorizedRefreshBehavior(ITokenRefresher tokenRefresher, IClientSession session)
    : IHttpPipelineStep
{
    public async Task<HttpResult> Execute(
        HttpCommand command,
        Func<HttpCommand, Task<HttpResult>> next,
        CancellationToken ct)
    {
        var result = await next(command);

        if (command.SkipUnauthorizedRefreshRetry
            || result.StatusCode != HttpStatusCode.Unauthorized
            || !command.IsAuthorize)
            return result;

        var refresh = await tokenRefresher.RefreshAsync(ct);
        if (refresh.IsFailed)
            return result;

        session.SetAccessToken(refresh.Value.AccessToken);
        RemoveHeader(command, "Authorization");

        command.SkipUnauthorizedRefreshRetry = true;
        return await next(command);
    }

    private static void RemoveHeader(HttpCommand command, string name)
    {
        var match = command.Headers.Keys.FirstOrDefault(k =>
            string.Equals(k, name, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
            command.Headers.Remove(match);
    }
}
