using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Home;

public enum ThemePreferencePick
{
    System,
    Light,
    Dark
}

public sealed class ThemeRow(ThemePreferencePick pick, string label)
{
    public ThemePreferencePick Pick { get; } = pick;
    public string Label { get; } = label;
}

public partial class SettingsViewModel(
    IWorkspaceShellNavigator workspaceShellNavigator,
    ILocalUserSettings localUserSettings,
    IThemeController themeController) : ViewModelBase
{
    private bool _suppressPersist;
    private bool _syncingRow;
    private CancellationTokenSource? _persistThemeCts;

    public ThemeRow[] ThemeRows { get; } =
    [
        new(ThemePreferencePick.System, Lang.Settings_ThemeSystem),
        new(ThemePreferencePick.Light, Lang.Settings_ThemeLight),
        new(ThemePreferencePick.Dark, Lang.Settings_ThemeDark)
    ];

    [ObservableProperty]
    public partial ThemePreferencePick SelectedTheme { get; set; }

    [ObservableProperty]
    public partial ThemeRow? SelectedThemeRow { get; set; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var stored = await localUserSettings.GetThemePreferenceAsync(cancellationToken).ConfigureAwait(false);
        var pick = ThemePreferenceMapper.ToPick(stored);

        _suppressPersist = true;
        _syncingRow = true;
        SelectedTheme = pick;
        SelectedThemeRow = ThemeRows.First(r => r.Pick == pick);
        _syncingRow = false;
        _suppressPersist = false;
    }

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
        catch (OperationCanceledException)
        {
            // A newer theme selection superseded this persistence request.
        }
    }

    [RelayCommand]
    private void Close() => workspaceShellNavigator.CloseSettings();

    public string PageTitle => Lang.Settings_PageTitle;

    public string ThemeSectionTitle => Lang.Settings_ThemeSection;

    public string CloseLabel => Lang.Settings_Back;
}
