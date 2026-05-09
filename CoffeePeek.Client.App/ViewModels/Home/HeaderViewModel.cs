using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Core.Identity;
using CoffeePeek.Client.App.Core.Security;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class HeaderViewModel : ViewModelBase
{
    private readonly IUserIdentityAccessor _userIdentityAccessor;
    private readonly IUserRoleAccessor _userRoleAccessor;
    private readonly IClientSession _clientSession;
    private readonly IWorkspaceShellNavigator _workspaceShellNavigator;
    private readonly IAccountSignOutService _accountSignOutService;

    public MainWorkspaceSectionCoordinator MainTabs { get; }

    [ObservableProperty]
    public partial string[] MainTabTitles { get; private set; }

    public HeaderViewModel(
        IUserIdentityAccessor userIdentityAccessor,
        IUserRoleAccessor userRoleAccessor,
        IClientSession clientSession,
        IWorkspaceShellNavigator workspaceShellNavigator,
        MainWorkspaceSectionCoordinator mainTabs,
        IAccountSignOutService accountSignOutService,
        ILocalizationService localizationService)
    {
        _userIdentityAccessor = userIdentityAccessor;
        _userRoleAccessor = userRoleAccessor;
        _clientSession = clientSession;
        _workspaceShellNavigator = workspaceShellNavigator;
        MainTabs = mainTabs;
        _accountSignOutService = accountSignOutService;
        MainTabTitles = BuildTabTitles();

        _clientSession.AccessTokenChanged += (_, _) => SyncRoleUi();
        SyncRoleUi();

        localizationService.LanguageChanged += (_, _) =>
        {
            MainTabTitles = BuildTabTitles();
        };

        MainTabs.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(MainWorkspaceSectionCoordinator.SelectedIndex))
                return;
            _workspaceShellNavigator.CloseUserProfile();
            _workspaceShellNavigator.CloseModerationPanel();
            _workspaceShellNavigator.CloseSettings();
        };
    }

    private static string[] BuildTabTitles() =>
    [
        Lang.Header_Catalog,
        Lang.Header_Favorites,
        Lang.Header_NearMe
    ];

    private void SyncRoleUi() =>
        IsModerator = _userRoleAccessor.IsInRole(WellKnownAppRoles.Moderator);

    [ObservableProperty]
    public partial bool IsModerator { get; private set; }

    [RelayCommand]
    private void SelectHeaderTab(object? parameter)
    {
        var tabIndex = parameter switch
        {
            int i => i,
            string s when int.TryParse(s, out var n) => n,
            _ => -1
        };

        if (tabIndex < 0 || tabIndex >= MainTabTitles.Length)
            return;

        if (MainTabs.SelectedIndex == tabIndex)
            return;

        MainTabs.SelectedIndex = tabIndex;
        _workspaceShellNavigator.CloseUserProfile();
        _workspaceShellNavigator.CloseModerationPanel();
        _workspaceShellNavigator.CloseSettings();
    }

    [RelayCommand]
    private void NavigateToProfile()
    {
        _workspaceShellNavigator.CloseModerationPanel();
        _workspaceShellNavigator.CloseSettings();
        if (_userIdentityAccessor.GetCurrentUserIdOrNull() is not { } userId)
            return;

        _workspaceShellNavigator.OpenUserProfile(userId);
    }

    [RelayCommand]
    private void OpenSettings() => _workspaceShellNavigator.OpenSettings();

    [RelayCommand]
    private async Task SignOutAsync() => await _accountSignOutService.SignOutAsync();

    [RelayCommand]
    private void OpenModerationPanel()
    {
        if (!IsModerator)
            return;

        _workspaceShellNavigator.CloseUserProfile();
        _workspaceShellNavigator.CloseSettings();
        _workspaceShellNavigator.OpenModerationPanel();
    }
}
