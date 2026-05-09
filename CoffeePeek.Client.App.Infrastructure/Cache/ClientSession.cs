using CoffeePeek.Client.App.Core.Cache;

namespace CoffeePeek.Client.App.Infrastructure.Cache;

public sealed class ClientSession : IClientSession
{
    private readonly object _gate = new();
    private string? _accessToken;

    public event EventHandler? AccessTokenChanged;

    public string? AccessToken => Volatile.Read(ref _accessToken);

    public void SetAccessToken(string? token)
    {
        bool changed;
        lock (_gate)
        {
            changed = !string.Equals(_accessToken, token, StringComparison.Ordinal);
            if (changed)
                _accessToken = token;
        }
        if (changed)
            AccessTokenChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        bool changed;
        lock (_gate)
        {
            changed = _accessToken is not null;
            if (changed)
                _accessToken = null;
        }
        if (changed)
            AccessTokenChanged?.Invoke(this, EventArgs.Empty);
    }
}
