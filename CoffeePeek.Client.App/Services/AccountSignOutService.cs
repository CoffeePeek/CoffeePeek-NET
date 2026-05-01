using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;
using FluentResults;

namespace CoffeePeek.Client.App.Services;

public sealed class AccountSignOutService(
    IWebAuthenticationClient authenticationClient,
    IClientSession clientSession,
    ILocalUserSettings localUserSettings,
    INavigationService navigationService,
    IWorkspaceShellNavigator workspaceShellNavigator) : IAccountSignOutService
{
    public async Task<Result> SignOutAsync(CancellationToken cancellationToken = default)
    {
        var logoutResult = await authenticationClient.Logout(cancellationToken);
        workspaceShellNavigator.CloseUserProfile();
        workspaceShellNavigator.CloseSettings();
        clientSession.Clear();
        await localUserSettings.ClearAsync(cancellationToken);
        navigationService.NavigateTo<WelcomePageViewModel>();

        return logoutResult.IsFailed
            ? Result.Fail(logoutResult.Errors)
            : Result.Ok();
    }
}
