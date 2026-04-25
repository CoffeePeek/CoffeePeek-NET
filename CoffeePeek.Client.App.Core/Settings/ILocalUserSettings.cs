namespace CoffeePeek.Client.App.Core.Settings;

/// <summary>
/// Persisted user preferences and secrets on the device (access token, etc.).
/// </summary>
public interface ILocalUserSettings
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    Task SetAccessTokenAsync(string? accessToken, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
