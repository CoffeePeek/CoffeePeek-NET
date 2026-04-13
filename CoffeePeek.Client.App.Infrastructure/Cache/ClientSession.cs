using CoffeePeek.Client.App.Core.Cache;

namespace CoffeePeek.Client.App.Infrastructure.Cache;

public sealed class ClientSession : IClientSession
{
    private string? _accessToken;

    public string? AccessToken => _accessToken;

    public void SetAccessToken(string? token) => _accessToken = token;

    public void Clear() => _accessToken = null;
}
