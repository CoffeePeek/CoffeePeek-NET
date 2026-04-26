using Avalonia.Styling;
using CoffeePeek.Client.App.Core.Execution;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Services;

namespace CoffeePeek.Client.App.Startup;

/// <summary>
/// Applies theme from local settings before the main shell is shown.
/// </summary>
public sealed class ApplyPersistedThemeExecutor(ILocalUserSettings localSettings, IThemeController themeController)
    : IBeforeMainShellExecutor
{
    public int Order => -900;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var pref = await localSettings.GetThemePreferenceAsync(cancellationToken).ConfigureAwait(false);
        var variant = pref switch
        {
            "Dark" => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };

        themeController.ApplyTheme(variant);
    }
}
