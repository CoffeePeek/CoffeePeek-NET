using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;

namespace CoffeePeek.Client.App.Services;

public sealed class AccountSignOutService(
    IWebAuthenticationClient authenticationClient,
    IClientSession clientSession,
    ILocalUserSettings localUserSettings,
    INavigationService navigationService,
    IWorkspaceShellNavigator workspaceShellNavigator) : IAccountSignOutService
{
    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        workspaceShellNavigator.CloseUserProfile();
        workspaceShellNavigator.CloseSettings();
        _ = await authenticationClient.Logout();
        clientSession.Clear();
        await localUserSettings.ClearAsync(cancellationToken).ConfigureAwait(false);
        navigationService.NavigateTo<WelcomePageViewModel>();
    }
}
