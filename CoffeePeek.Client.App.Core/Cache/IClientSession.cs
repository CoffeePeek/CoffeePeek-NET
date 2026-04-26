namespace CoffeePeek.Client.App.Core.Cache;

public interface IClientSession
{
    event EventHandler? AccessTokenChanged;

    string? AccessToken { get; }

    void SetAccessToken(string? token);

    void Clear();
}
