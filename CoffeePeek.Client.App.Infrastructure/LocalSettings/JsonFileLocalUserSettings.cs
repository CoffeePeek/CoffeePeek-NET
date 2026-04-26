using System.Text.Json;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;

namespace CoffeePeek.Client.App.Infrastructure.LocalSettings;

public sealed class JsonFileLocalUserSettings(LocalUserSettingsOptions options) : ILocalUserSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var doc = await LoadDocumentAsync(cancellationToken).ConfigureAwait(false);
        return doc?.AccessToken;
    }

    public async Task SetAccessTokenAsync(string? accessToken, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var doc = await LoadDocumentUnlockedAsync(cancellationToken).ConfigureAwait(false);
            doc.AccessToken = accessToken;
            await SaveDocumentUnlockedAsync(doc, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<string?> GetThemePreferenceAsync(CancellationToken cancellationToken = default)
    {
        var doc = await LoadDocumentAsync(cancellationToken).ConfigureAwait(false);
        return NormalizeTheme(doc?.Theme);
    }

    public async Task SetThemePreferenceAsync(string? themeLightDarkOrNull, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var doc = await LoadDocumentUnlockedAsync(cancellationToken).ConfigureAwait(false);
            doc.Theme = NormalizeTheme(themeLightDarkOrNull);
            await SaveDocumentUnlockedAsync(doc, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var path = GetFilePath();
            if (File.Exists(path))
                File.Delete(path);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<UserSettingsDocument> LoadDocumentAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await LoadDocumentUnlockedAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<UserSettingsDocument> LoadDocumentUnlockedAsync(CancellationToken cancellationToken)
    {
        var path = GetFilePath();
        if (!File.Exists(path))
            return new UserSettingsDocument();

        await using var stream = File.OpenRead(path);
        var doc = await JsonSerializer.DeserializeAsync<UserSettingsDocument>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return doc ?? new UserSettingsDocument();
    }

    private async Task SaveDocumentUnlockedAsync(UserSettingsDocument doc, CancellationToken cancellationToken)
    {
        var path = GetFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(doc, JsonOptions), cancellationToken)
            .ConfigureAwait(false);
    }

    private string GetFilePath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, options.AppFolderName, options.SettingsFileName);
    }

    private static string? NormalizeTheme(string? theme)
    {
        if (string.IsNullOrWhiteSpace(theme))
            return null;

        if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
            return "Light";

        if (string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase))
            return "Dark";

        return null;
    }

    private sealed class UserSettingsDocument
    {
        public string? AccessToken { get; set; }

        /// <summary>Light, Dark, or omitted/null for system.</summary>
        public string? Theme { get; set; }
    }
}
