namespace CoffeePeek.Client.App.Core.Cache;

public interface IClientSession
{
    string? AccessToken { get; }

    void SetAccessToken(string? token);

    void Clear();
}
