namespace CoffeePeek.Client.App.Services;

public interface ILocalizationService
{
    string CurrentLanguage { get; }
    event EventHandler? LanguageChanged;
    void ApplyLanguage(string languageCode);
}
