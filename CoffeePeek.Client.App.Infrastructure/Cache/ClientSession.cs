using CoffeePeek.Client.App.Core.Cache;

namespace CoffeePeek.Client.App.Infrastructure.Cache;

public sealed class ClientSession : IClientSession
{
    private string? _accessToken;

    public event EventHandler? AccessTokenChanged;

    public string? AccessToken => _accessToken;

    public void SetAccessToken(string? token)
    {
        _accessToken = token;
        AccessTokenChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _accessToken = null;
        AccessTokenChanged?.Invoke(this, EventArgs.Empty);
    }
}
