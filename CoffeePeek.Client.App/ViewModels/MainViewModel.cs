using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;
using LoginViewModel = CoffeePeek.Client.App.ViewModels.Auth.LoginViewModel;

namespace CoffeePeek.Client.App.ViewModels;

public sealed class MainViewModel(LoginViewModel loginViewModel, WelcomePageViewModel welcomePageViewModel) : ViewModelBase
{
    public LoginViewModel LoginViewModel { get; } = loginViewModel;
    public WelcomePageViewModel WelcomePageViewModel { get; } = welcomePageViewModel;
}