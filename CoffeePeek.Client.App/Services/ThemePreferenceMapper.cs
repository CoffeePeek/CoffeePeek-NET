using Avalonia.Styling;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.ViewModels.Home;

namespace CoffeePeek.Client.App.Services;

public static class ThemePreferenceMapper
{
    public static ThemeVariant ToVariant(string? value) =>
        value switch
        {
            ThemePreferenceValues.Dark => ThemeVariant.Dark,
            ThemePreferenceValues.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };

    public static string? ToStorageValue(ThemePreferencePick pick) =>
        pick switch
        {
            ThemePreferencePick.Dark => ThemePreferenceValues.Dark,
            ThemePreferencePick.Light => ThemePreferenceValues.Light,
            _ => null
        };

    public static ThemePreferencePick ToPick(string? value) =>
        value switch
        {
            ThemePreferenceValues.Dark => ThemePreferencePick.Dark,
            ThemePreferenceValues.Light => ThemePreferencePick.Light,
            _ => ThemePreferencePick.System
        };
}
