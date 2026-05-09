using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;

namespace CoffeePeek.Client.App.Services;

public sealed class ThemeController : IThemeController
{
    public void ToggleLightDark()
    {
        if (Application.Current is not { } app)
            return;

        ApplyTheme(app.ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark);
    }

    public void ApplyTheme(ThemeVariant variant)
    {
        void Apply()
        {
            if (Application.Current is not { } app)
                return;

            app.RequestedThemeVariant = variant;
        }

        if (Dispatcher.UIThread.CheckAccess())
            Apply();
        else
            Dispatcher.UIThread.Post(Apply);
    }
}
