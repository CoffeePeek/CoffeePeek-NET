using System.Collections.ObjectModel;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;

public sealed partial class WelcomePageViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    public WelcomePageViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card1Title, Resources.Lang.Resources.WelcomePage_Card1Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card2Title, Resources.Lang.Resources.WelcomePage_Card2Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card3Title, Resources.Lang.Resources.WelcomePage_Card3Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card4Title, Resources.Lang.Resources.WelcomePage_Card4Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card5Title, Resources.Lang.Resources.WelcomePage_Card5Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card6Title, Resources.Lang.Resources.WelcomePage_Card6Description));
    }

    public ObservableCollection<WelcomePageCardItemViewModel> Features { get; } = [];

    [RelayCommand]
    private void GoToLogin() => _navigation.NavigateTo<LoginViewModel>();

    [RelayCommand]
    private void GoToRegister() => _navigation.NavigateTo<RegisterViewModel>();
}
