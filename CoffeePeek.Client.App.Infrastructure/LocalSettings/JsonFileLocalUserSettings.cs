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
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var path = GetFilePath();
            if (!File.Exists(path))
                return null;

            await using var stream = File.OpenRead(path);
            var doc = await JsonSerializer.DeserializeAsync<UserSettingsDocument>(stream, JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            return doc?.AccessToken;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SetAccessTokenAsync(string? accessToken, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var path = GetFilePath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var doc = new UserSettingsDocument { AccessToken = accessToken };
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(doc, JsonOptions), cancellationToken)
                .ConfigureAwait(false);
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

    private string GetFilePath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, options.AppFolderName, options.SettingsFileName);
    }

    private sealed class UserSettingsDocument
    {
        public string? AccessToken { get; set; }
    }
}
