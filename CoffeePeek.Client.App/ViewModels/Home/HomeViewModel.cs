using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.Home;

public sealed partial class HomeViewModel(
    IClientSession session,
    INavigationService navigation,
    IWebAuthenticationClient authClient,
    ILocalUserSettings localUserSettings) : ViewModelBase
{
    [RelayCommand]
    private async Task SignOutAsync()
    {
        _ = await authClient.Logout();
        session.Clear();
        await localUserSettings.ClearAsync();
        navigation.NavigateTo<WelcomePageViewModel>();
    }
}
