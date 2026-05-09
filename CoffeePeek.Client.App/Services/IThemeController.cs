using Avalonia.Styling;

namespace CoffeePeek.Client.App.Services;

public interface IThemeController
{
    void ToggleLightDark();

    void ApplyTheme(ThemeVariant variant);
}
