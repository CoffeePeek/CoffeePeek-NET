using CoffeePeek.Client.App.Core.Cache;

namespace CoffeePeek.Client.App.Infrastructure.Cache;

public sealed class ClientSession : IClientSession
{
    private string? _accessToken;

    public event EventHandler? AccessTokenChanged;

    public string? AccessToken => _accessToken;

    public void SetAccessToken(string? token)
    {
        if (string.Equals(_accessToken, token, StringComparison.Ordinal))
            return;

        _accessToken = token;
        AccessTokenChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        if (_accessToken is null)
            return;

        _accessToken = null;
        AccessTokenChanged?.Invoke(this, EventArgs.Empty);
    }
}
