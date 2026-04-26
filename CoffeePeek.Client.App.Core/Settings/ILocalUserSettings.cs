namespace CoffeePeek.Client.App.Core.Settings;

/// <summary>
/// Persisted user preferences and secrets on the device (access token, etc.).
/// </summary>
public interface ILocalUserSettings
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    Task SetAccessTokenAsync(string? accessToken, CancellationToken cancellationToken = default);

    /// <summary>Returns <c>Light</c>, <c>Dark</c>, or <c>null</c> for system default.</summary>
    Task<string?> GetThemePreferenceAsync(CancellationToken cancellationToken = default);

    Task SetThemePreferenceAsync(string? themeLightDarkOrNull, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
