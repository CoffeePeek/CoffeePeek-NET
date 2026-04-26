using CoffeePeek.Client.App.Core.Identity;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class HeaderViewModel(
    IUserIdentityAccessor userIdentityAccessor,
    IWorkspaceShellNavigator workspaceShellNavigator,
    MainWorkspaceSectionCoordinator mainTabs,
    IAccountSignOutService accountSignOutService) : ViewModelBase
{
    public MainWorkspaceSectionCoordinator MainTabs { get; } = mainTabs;

    public string[] MainTabTitles { get; } =
    [
        Lang.Header_Catalog,
        Lang.Header_Favorites,
        Lang.Header_NearMe
    ];

    [RelayCommand]
    private void SelectHeaderTab(object? parameter)
    {
        var tabIndex = parameter switch
        {
            int i => i,
            string s when int.TryParse(s, out var n) => n,
            _ => 0
        };

        if (MainTabs.SelectedIndex == tabIndex)
            return;

        MainTabs.SelectedIndex = tabIndex;
        workspaceShellNavigator.CloseUserProfile();
    }

    [RelayCommand]
    private void NavigateToProfile()
    {
        if (userIdentityAccessor.GetCurrentUserIdOrNull() is not { } userId)
            return;

        workspaceShellNavigator.OpenUserProfile(userId);
    }

    [RelayCommand]
    private void OpenSettings() => workspaceShellNavigator.OpenSettings();

    [RelayCommand]
    private async Task SignOutAsync() => await accountSignOutService.SignOutAsync();
}
