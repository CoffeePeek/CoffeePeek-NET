using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Execution;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;

namespace CoffeePeek.Client.App.Startup;

/// <summary>
/// Chooses welcome vs. signed-in shell before the main window is shown.
/// </summary>
public sealed class InitialRouteExecutor(IClientSession session, INavigationService navigation) : IBeforeMainShellExecutor
{
    public int Order => 100;

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(session.AccessToken))
            navigation.NavigateTo<WelcomePageViewModel>();
        else
            navigation.Reset();

        return Task.CompletedTask;
    }
}
