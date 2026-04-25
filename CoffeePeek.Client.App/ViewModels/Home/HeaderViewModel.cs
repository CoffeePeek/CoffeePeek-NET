using CoffeePeek.Client.App.Core.Identity;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class HeaderViewModel(
    IUserIdentityAccessor userIdentityAccessor,
    IWorkspaceShellNavigator workspaceShellNavigator) : ViewModelBase
{
    [ObservableProperty]
    public partial int MainTabSelectedIndex { get; set; }

    partial void OnMainTabSelectedIndexChanged(int value)
    {
        if (value >= 0)
            workspaceShellNavigator.CloseUserProfile();
    }

    [RelayCommand]
    private void CloseProfileShell()
    {
        workspaceShellNavigator.CloseUserProfile();
    }

    [RelayCommand]
    private void NavigateToProfile()
    {
        if (userIdentityAccessor.GetCurrentUserIdOrNull() is not { } userId)
            return;

        workspaceShellNavigator.OpenUserProfile(userId);
    }
}