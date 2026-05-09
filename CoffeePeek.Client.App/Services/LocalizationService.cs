using System.Globalization;
using CoffeePeek.Client.App.Core.Settings;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.Services;

public sealed class LocalizationService : ILocalizationService
{
    public string CurrentLanguage { get; private set; } = LanguageValues.English;

    public event EventHandler? LanguageChanged;

    public void ApplyLanguage(string languageCode)
    {
        if (string.Equals(CurrentLanguage, languageCode, StringComparison.OrdinalIgnoreCase))
            return;

        CurrentLanguage = languageCode;
        Lang.Culture = new CultureInfo(languageCode);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
