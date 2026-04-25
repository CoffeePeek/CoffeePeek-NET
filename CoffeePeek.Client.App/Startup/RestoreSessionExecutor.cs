using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Execution;
using CoffeePeek.Client.App.Core.Settings;

namespace CoffeePeek.Client.App.Startup;

/// <summary>
/// Hydrates <see cref="IClientSession"/> from persisted local settings before routing.
/// </summary>
public sealed class RestoreSessionExecutor(IClientSession session, ILocalUserSettings localSettings)
    : IBeforeMainShellExecutor
{
    public int Order => -1000;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var token = await localSettings.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(token))
            session.SetAccessToken(token);
    }
}
