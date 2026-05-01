using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Shops;
using CommunityToolkit.Mvvm.ComponentModel;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class WorkspaceViewModel : ViewModelBase, IDisposable
{
    private readonly IWorkspaceShellNavigator _shellNavigator;

    public ShopsPageViewModel ShopPage { get; }

    public UserProfileViewModel UserProfile { get; }

    public ShopDetailViewModel ShopDetail { get; }

    public SuggestShopViewModel SuggestShop { get; }

    public SettingsViewModel Settings { get; }

    public ModerationPanelViewModel ModerationPanel { get; }

    public PlaceholderPageViewModel FavoritesPlaceholder { get; } = new();

    public PlaceholderPageViewModel NearMePlaceholder { get; } = new();

    [ObservableProperty]
    public partial bool IsProfileOpen { get; private set; }

    [ObservableProperty]
    public partial bool IsShopDetailOpen { get; private set; }

    [ObservableProperty]
    public partial bool IsSuggestShopOpen { get; private set; }

    [ObservableProperty]
    public partial bool IsSettingsOpen { get; private set; }

    [ObservableProperty]
    public partial bool IsModerationPanelOpen { get; private set; }

    /// <summary>True when catalog or placeholder strip should show (not profile/detail/suggest/settings/moderation).</summary>
    public bool IsMainBrowseVisible =>
        !IsProfileOpen && !IsShopDetailOpen && !IsSuggestShopOpen && !IsSettingsOpen && !IsModerationPanelOpen;

    public bool IsCatalogVisible => IsMainBrowseVisible && MainTabs.Section == MainWorkspaceSection.Catalog;

    public bool IsFavoritesPlaceholderVisible =>
        IsMainBrowseVisible && MainTabs.Section == MainWorkspaceSection.Favorites;

    public bool IsNearMePlaceholderVisible =>
        IsMainBrowseVisible && MainTabs.Section == MainWorkspaceSection.NearMe;

    public MainWorkspaceSectionCoordinator MainTabs { get; }

    public WorkspaceViewModel(
        ShopsPageViewModel shopsPageViewModel,
        UserProfileViewModel userProfileViewModel,
        ShopDetailViewModel shopDetailViewModel,
        SuggestShopViewModel suggestShopViewModel,
        SettingsViewModel settingsViewModel,
        ModerationPanelViewModel moderationPanelViewModel,
        MainWorkspaceSectionCoordinator mainTabs,
        IWorkspaceShellNavigator shellNavigator)
    {
        ShopPage = shopsPageViewModel;
        UserProfile = userProfileViewModel;
        ShopDetail = shopDetailViewModel;
        SuggestShop = suggestShopViewModel;
        Settings = settingsViewModel;
        ModerationPanel = moderationPanelViewModel;
        MainTabs = mainTabs;
        _shellNavigator = shellNavigator;

        FavoritesPlaceholder.Title = Lang.Placeholder_Favorites_Title;
        FavoritesPlaceholder.Message = Lang.Placeholder_Favorites_Message;
        NearMePlaceholder.Title = Lang.Placeholder_NearMe_Title;
        NearMePlaceholder.Message = Lang.Placeholder_NearMe_Message;

        _ = ShopPage.InitializeAsync();

        MainTabs.PropertyChanged += OnMainTabsPropertyChanged;

        shellNavigator.AttachProfile(
            id =>
            {
                IsShopDetailOpen = false;
                IsSuggestShopOpen = false;
                IsSettingsOpen = false;
                IsModerationPanelOpen = false;
                IsProfileOpen = true;
                _ = UserProfile.LoadAsync(id);
            },
            () =>
            {
                IsProfileOpen = false;
            });

        shellNavigator.AttachShopDetail(
            shopId =>
            {
                IsProfileOpen = false;
                IsSuggestShopOpen = false;
                IsSettingsOpen = false;
                IsModerationPanelOpen = false;
                IsShopDetailOpen = true;
                _ = ShopDetail.LoadAsync(shopId);
            },
            () =>
            {
                IsShopDetailOpen = false;
            });

        shellNavigator.AttachSuggestShop(
            () =>
            {
                IsProfileOpen = false;
                IsShopDetailOpen = false;
                IsSettingsOpen = false;
                IsModerationPanelOpen = false;
                IsSuggestShopOpen = true;
                _ = SuggestShop.InitializeAsync();
            },
            () =>
            {
                IsSuggestShopOpen = false;
            });

        shellNavigator.AttachSettings(
            () =>
            {
                IsProfileOpen = false;
                IsShopDetailOpen = false;
                IsSuggestShopOpen = false;
                IsModerationPanelOpen = false;
                IsSettingsOpen = true;
                _ = Settings.LoadAsync();
            },
            () =>
            {
                IsSettingsOpen = false;
            });

        shellNavigator.AttachModerationPanel(
            () =>
            {
                IsProfileOpen = false;
                IsShopDetailOpen = false;
                IsSuggestShopOpen = false;
                IsSettingsOpen = false;
                IsModerationPanelOpen = true;
                _ = ModerationPanel.LoadAsync();
            },
            () =>
            {
                IsModerationPanelOpen = false;
            });
    }

    private void OnMainTabsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainWorkspaceSectionCoordinator.Section))
            return;

        _shellNavigator.CloseUserProfile();
        _shellNavigator.CloseModerationPanel();
        _shellNavigator.CloseSettings();
        RefreshBrowseVisibility();
    }

    public void Dispose()
    {
        MainTabs.PropertyChanged -= OnMainTabsPropertyChanged;
    }

    private void RefreshBrowseVisibility()
    {
        OnPropertyChanged(nameof(IsMainBrowseVisible));
        OnPropertyChanged(nameof(IsCatalogVisible));
        OnPropertyChanged(nameof(IsFavoritesPlaceholderVisible));
        OnPropertyChanged(nameof(IsNearMePlaceholderVisible));
    }

    partial void OnIsProfileOpenChanged(bool value) => RefreshBrowseVisibility();

    partial void OnIsShopDetailOpenChanged(bool value) => RefreshBrowseVisibility();

    partial void OnIsSuggestShopOpenChanged(bool value) => RefreshBrowseVisibility();

    partial void OnIsSettingsOpenChanged(bool value) => RefreshBrowseVisibility();

    partial void OnIsModerationPanelOpenChanged(bool value) => RefreshBrowseVisibility();
}
