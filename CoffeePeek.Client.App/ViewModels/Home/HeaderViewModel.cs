using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Identity;
using CoffeePeek.Client.App.Core.Security;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class HeaderViewModel : ViewModelBase
{
    private readonly IUserIdentityAccessor _userIdentityAccessor;
    private readonly IUserRoleAccessor _userRoleAccessor;
    private readonly IClientSession _clientSession;
    private readonly IWorkspaceShellNavigator _workspaceShellNavigator;

    public HeaderViewModel(
        IUserIdentityAccessor userIdentityAccessor,
        IUserRoleAccessor userRoleAccessor,
        IClientSession clientSession,
        IWorkspaceShellNavigator workspaceShellNavigator)
    {
        _userIdentityAccessor = userIdentityAccessor;
        _userRoleAccessor = userRoleAccessor;
        _clientSession = clientSession;
        _workspaceShellNavigator = workspaceShellNavigator;
        _clientSession.AccessTokenChanged += (_, _) => SyncRoleUi();
        SyncRoleUi();
    }

    [ObservableProperty]
    public partial int MainTabSelectedIndex { get; set; }

    [ObservableProperty]
    public partial bool IsModerator { get; private set; }

    partial void OnMainTabSelectedIndexChanged(int value)
    {
        if (value < 0)
            return;
        _workspaceShellNavigator.CloseUserProfile();
        _workspaceShellNavigator.CloseModerationPanel();
    }

    [RelayCommand]
    private void CloseProfileShell()
    {
        _workspaceShellNavigator.CloseUserProfile();
        _workspaceShellNavigator.CloseModerationPanel();
    }

    [RelayCommand]
    private void NavigateToProfile()
    {
        _workspaceShellNavigator.CloseModerationPanel();
        if (_userIdentityAccessor.GetCurrentUserIdOrNull() is not { } userId)
            return;

        _workspaceShellNavigator.OpenUserProfile(userId);
    }

    [RelayCommand]
    private void OpenModerationPanel()
    {
        if (!IsModerator)
            return;

        _workspaceShellNavigator.CloseUserProfile();
        _workspaceShellNavigator.OpenModerationPanel();
    }

    public void SyncRoleUi() =>
        IsModerator = _userRoleAccessor.IsInRole(WellKnownAppRoles.Moderator);
}
