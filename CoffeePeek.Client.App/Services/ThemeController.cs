using Avalonia;
using Avalonia.Styling;

namespace CoffeePeek.Client.App.Services;

public sealed class ThemeController : IThemeController
{
    public void ToggleLightDark()
    {
        if (Application.Current is not { } app)
            return;

        app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
    }
}
