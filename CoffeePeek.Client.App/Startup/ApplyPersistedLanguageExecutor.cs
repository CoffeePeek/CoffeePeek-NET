using CoffeePeek.Client.App.Core.Execution;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Services;

namespace CoffeePeek.Client.App.Startup;

public sealed class ApplyPersistedLanguageExecutor(ILocalUserSettings localSettings, ILocalizationService localizationService)
    : IBeforeMainShellExecutor
{
    public int Order => BeforeMainShellOrders.ApplyPersistedLanguage;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var lang = await localSettings.GetLanguageAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(lang))
            localizationService.ApplyLanguage(lang);
    }
}
