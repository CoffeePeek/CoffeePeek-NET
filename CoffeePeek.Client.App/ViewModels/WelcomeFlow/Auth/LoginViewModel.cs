using Res = CoffeePeek.Client.App.Resources.Lang.Resources;
using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Auth;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;

public sealed partial class LoginViewModel(
    INavigationService navigationService,
    IClientSession session,
    IWebAuthenticationClient authClient,
    ILocalUserSettings localUserSettings,
    IThemeController themeController) : ViewModelBase
{
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool ThemeShowsSunIcon =>
        Avalonia.Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));

    [RelayCommand]
    private void GoBack() => navigationService.NavigateTo<WelcomePageViewModel>();

    [RelayCommand]
    private void ToggleTheme()
    {
        themeController.ToggleLightDark();
        OnPropertyChanged(nameof(ThemeShowsSunIcon));
    }

    [RelayCommand]
    private void GoToRegister() =>
        navigationService.NavigateTo<RegisterViewModel>(r => r.ResetForEmailStep());

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = Res.Auth_FillAllFields;
            return;
        }

        var result = await authClient.GetToken(
            new GetTokenRequest { Email = Email.Trim(), Password = Password });

        if (result.IsFailed || string.IsNullOrEmpty(result.Value?.AccessToken))
        {
            ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Res.Auth_LoginFailed;
            return;
        }

        session.SetAccessToken(result.Value.AccessToken);
        await localUserSettings.SetAccessTokenAsync(result.Value.AccessToken);
        Password = string.Empty;
        navigationService.Reset();
    }
}
