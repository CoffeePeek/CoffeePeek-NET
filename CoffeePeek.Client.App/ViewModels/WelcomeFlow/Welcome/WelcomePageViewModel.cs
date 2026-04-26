using Avalonia;
using Avalonia.Styling;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;

public sealed partial class WelcomePageViewModel(INavigationService navigation, IThemeController themeController)
    : ViewModelBase
{
    public static bool ThemeShowsSunIcon =>
        Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

    [RelayCommand]
    private void GoToLogin() => navigation.NavigateTo<LoginViewModel>();

    [RelayCommand]
    private void GoToRegister() =>
        navigation.NavigateTo<RegisterViewModel>(r => r.ResetForEmailStep());

    [RelayCommand]
    private void ToggleTheme()
    {
        themeController.ToggleLightDark();
        OnPropertyChanged(nameof(ThemeShowsSunIcon));
    }
}
