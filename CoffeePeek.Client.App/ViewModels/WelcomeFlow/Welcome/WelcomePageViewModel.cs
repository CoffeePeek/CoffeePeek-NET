using System.Collections.ObjectModel;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;

public sealed partial class WelcomePageViewModel : ViewModelBase
{
    private readonly IAuthNavigation _navigation;

    public WelcomePageViewModel(IAuthNavigation navigation)
    {
        _navigation = navigation;
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card1Title, Resources.Lang.Resources.WelcomePage_Card1Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card2Title, Resources.Lang.Resources.WelcomePage_Card2Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card3Title, Resources.Lang.Resources.WelcomePage_Card3Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card4Title, Resources.Lang.Resources.WelcomePage_Card4Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card5Title, Resources.Lang.Resources.WelcomePage_Card5Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card6Title, Resources.Lang.Resources.WelcomePage_Card6Description));
    }

    /// <summary>Parameterless ctor for previewers; navigation commands are no-ops.</summary>
#pragma warning disable CS8618
    public WelcomePageViewModel() : this(DesignNavigation.Instance)
#pragma warning restore CS8618
    {
    }

    public ObservableCollection<WelcomePageCardItemViewModel> Features { get; } = [];

    [RelayCommand]
    private void GoToLogin() => _navigation.ShowLogin();

    [RelayCommand]
    private void GoToRegister() => _navigation.ShowRegisterEmail();

    private sealed class DesignNavigation : IAuthNavigation
    {
        public static readonly DesignNavigation Instance = new();

        public void ShowWelcome() { }

        public void ShowLogin(string? email = null) { }

        public void ShowRegisterEmail() { }

        public void ShowRegister(string email) { }

        public void ShowHome() { }
    }
}
