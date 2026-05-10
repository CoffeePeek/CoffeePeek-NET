using CoffeePeek.Client.App.Core.Identity;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Home;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.Mobile;

public partial class MobileShellViewModel : ViewModelBase
{
    private readonly IWorkspaceShellNavigator _shellNavigator;
    private readonly IUserIdentityAccessor _identityAccessor;
    private readonly HeaderViewModel _headerViewModel;

    public WorkspaceViewModel Workspace { get; }

    public MobileShellViewModel(
        WorkspaceViewModel workspace,
        IWorkspaceShellNavigator shellNavigator,
        IUserIdentityAccessor identityAccessor,
        HeaderViewModel headerViewModel)
    {
        Workspace = workspace;
        _shellNavigator = shellNavigator;
        _identityAccessor = identityAccessor;
        _headerViewModel = headerViewModel;

        workspace.PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName);
        headerViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HeaderViewModel.IsModerator))
                OnPropertyChanged(nameof(IsModerator));
        };
    }

    // Pass-through visibility properties that delegate to workspace
    public bool IsMainBrowseVisible => Workspace.IsMainBrowseVisible;
    public bool IsCatalogVisible => Workspace.IsCatalogVisible;
    public bool IsNearMePlaceholderVisible => Workspace.IsNearMePlaceholderVisible;
    public bool IsShopDetailOpen => Workspace.IsShopDetailOpen;
    public bool IsProfileOpen => Workspace.IsProfileOpen;
    public bool IsSettingsOpen => Workspace.IsSettingsOpen;
    public bool IsModerationPanelOpen => Workspace.IsModerationPanelOpen;

    public bool IsModerator => _headerViewModel.IsModerator;

    [RelayCommand]
    private void SelectCatalog()
    {
        CloseAllOverlays();
        Workspace.MainTabs.SelectedIndex = 0;
    }

    [RelayCommand]
    private void SelectNearMe()
    {
        CloseAllOverlays();
        Workspace.MainTabs.SelectedIndex = 2;
    }

    [RelayCommand]
    private void SelectProfile()
    {
        if (_identityAccessor.GetCurrentUserIdOrNull() is { } userId)
            _shellNavigator.OpenUserProfile(userId);
    }

    [RelayCommand]
    private void SelectModeration()
    {
        _shellNavigator.OpenModerationPanel();
    }

    private void CloseAllOverlays()
    {
        _shellNavigator.CloseUserProfile();
        _shellNavigator.CloseShopDetail();
        _shellNavigator.CloseSuggestShop();
        _shellNavigator.CloseSettings();
        _shellNavigator.CloseModerationPanel();
    }
}
