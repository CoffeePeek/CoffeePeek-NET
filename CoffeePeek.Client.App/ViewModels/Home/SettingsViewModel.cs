using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Home;

public enum ThemePreferencePick { System, Light, Dark }

public sealed class ThemeRow(ThemePreferencePick pick, string label)
{
    public ThemePreferencePick Pick { get; } = pick;
    public string Label { get; } = label;
}

public sealed class LanguageRow(string code, string label)
{
    public string Code { get; } = code;
    public string Label { get; } = label;
}

public partial class SettingsViewModel(
    IWorkspaceShellNavigator workspaceShellNavigator,
    ILocalUserSettings localUserSettings,
    IThemeController themeController,
    ILocalizationService localizationService) : ViewModelBase
{
    private bool _suppressPersist;
    private bool _syncingRow;
    private CancellationTokenSource? _persistThemeCts;
    private CancellationTokenSource? _persistLangCts;

    // ── Theme ────────────────────────────────────────────────────────────────

    [ObservableProperty]
    public partial ThemeRow[] ThemeRows { get; private set; } = BuildThemeRows();

    [ObservableProperty]
    public partial ThemePreferencePick SelectedTheme { get; set; }

    [ObservableProperty]
    public partial ThemeRow? SelectedThemeRow { get; set; }

    // ── Language ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    public partial LanguageRow[] LanguageRows { get; private set; } = BuildLanguageRows();

    [ObservableProperty]
    public partial LanguageRow? SelectedLanguageRow { get; set; }

    // ── Labels (refreshed on language change) ────────────────────────────────

    [ObservableProperty]
    public partial string PageTitle { get; private set; } = Lang.Settings_PageTitle;

    [ObservableProperty]
    public partial string ThemeSectionTitle { get; private set; } = Lang.Settings_ThemeSection;

    [ObservableProperty]
    public partial string LanguageSectionTitle { get; private set; } = Lang.Settings_LanguageSection;

    [ObservableProperty]
    public partial string CloseLabel { get; private set; } = Lang.Settings_Back;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        // Theme
        var storedTheme = await localUserSettings.GetThemePreferenceAsync(cancellationToken).ConfigureAwait(false);
        var pick = ThemePreferenceMapper.ToPick(storedTheme);

        _suppressPersist = true;
        _syncingRow = true;
        SelectedTheme = pick;
        SelectedThemeRow = ThemeRows.First(r => r.Pick == pick);
        _syncingRow = false;
        _suppressPersist = false;

        // Language
        var storedLang = await localUserSettings.GetLanguageAsync(cancellationToken).ConfigureAwait(false);
        var langCode = storedLang ?? localizationService.CurrentLanguage;
        _suppressPersist = true;
        SelectedLanguageRow = LanguageRows.FirstOrDefault(r => r.Code == langCode) ?? LanguageRows[0];
        _suppressPersist = false;

        // Refresh all labels now that language is resolved
        localizationService.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Rebuild rows with new labels
        _suppressPersist = true;
        _syncingRow = true;
        var currentTheme = SelectedTheme;
        var currentLangCode = SelectedLanguageRow?.Code ?? localizationService.CurrentLanguage;
        ThemeRows = BuildThemeRows();
        LanguageRows = BuildLanguageRows();
        SelectedThemeRow = ThemeRows.First(r => r.Pick == currentTheme);
        SelectedLanguageRow = LanguageRows.FirstOrDefault(r => r.Code == currentLangCode) ?? LanguageRows[0];
        _syncingRow = false;
        _suppressPersist = false;

        // Refresh label properties
        PageTitle = Lang.Settings_PageTitle;
        ThemeSectionTitle = Lang.Settings_ThemeSection;
        LanguageSectionTitle = Lang.Settings_LanguageSection;
        CloseLabel = Lang.Settings_Back;
    }

    // ── Theme change ─────────────────────────────────────────────────────────

    partial void OnSelectedThemeChanged(ThemePreferencePick value)
    {
        if (!_syncingRow)
        {
            _syncingRow = true;
            SelectedThemeRow = ThemeRows.First(r => r.Pick == value);
            _syncingRow = false;
        }

        if (_suppressPersist)
            return;

        _persistThemeCts?.Cancel();
        _persistThemeCts?.Dispose();
        _persistThemeCts = new CancellationTokenSource();
        _ = PersistThemeAsync(value, _persistThemeCts.Token);
    }

    partial void OnSelectedThemeRowChanged(ThemeRow? value)
    {
        if (_syncingRow || value is null)
            return;

        if (SelectedTheme != value.Pick)
            SelectedTheme = value.Pick;
    }

    private async Task PersistThemeAsync(ThemePreferencePick pick, CancellationToken cancellationToken)
    {
        try
        {
            var stored = ThemePreferenceMapper.ToStorageValue(pick);
            await localUserSettings.SetThemePreferenceAsync(stored, cancellationToken).ConfigureAwait(false);
            themeController.ApplyTheme(ThemePreferenceMapper.ToVariant(stored));
        }
        catch (OperationCanceledException) { }
    }

    // ── Language change ───────────────────────────────────────────────────────

    partial void OnSelectedLanguageRowChanged(LanguageRow? value)
    {
        if (_suppressPersist || value is null)
            return;

        if (value.Code == localizationService.CurrentLanguage)
            return;

        _persistLangCts?.Cancel();
        _persistLangCts?.Dispose();
        _persistLangCts = new CancellationTokenSource();
        _ = PersistLanguageAsync(value.Code, _persistLangCts.Token);
    }

    private async Task PersistLanguageAsync(string code, CancellationToken cancellationToken)
    {
        try
        {
            await localUserSettings.SetLanguageAsync(code, cancellationToken).ConfigureAwait(false);
            localizationService.ApplyLanguage(code);
        }
        catch (OperationCanceledException) { }
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void Close() => workspaceShellNavigator.CloseSettings();

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ThemeRow[] BuildThemeRows() =>
    [
        new(ThemePreferencePick.System, Lang.Settings_ThemeSystem),
        new(ThemePreferencePick.Light,  Lang.Settings_ThemeLight),
        new(ThemePreferencePick.Dark,   Lang.Settings_ThemeDark)
    ];

    private static LanguageRow[] BuildLanguageRows() =>
    [
        new(LanguageValues.English, Lang.Settings_Language_English),
        new(LanguageValues.Russian, Lang.Settings_Language_Russian)
    ];
}
