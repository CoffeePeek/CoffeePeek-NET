using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Shops;
using CommunityToolkit.Mvvm.ComponentModel;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class WorkspaceViewModel : ViewModelBase
{
    public ShopsPageViewModel ShopPage { get; }

    public UserProfileViewModel UserProfile { get; }

    public ShopDetailViewModel ShopDetail { get; }

    public SuggestShopViewModel SuggestShop { get; }

    public SettingsViewModel Settings { get; }

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

    /// <summary>True when catalog or placeholder strip should show (not profile/detail/suggest/settings).</summary>
    public bool IsMainBrowseVisible =>
        !IsProfileOpen && !IsShopDetailOpen && !IsSuggestShopOpen && !IsSettingsOpen;

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
        MainWorkspaceSectionCoordinator mainTabs,
        IWorkspaceShellNavigator shellNavigator)
    {
        ShopPage = shopsPageViewModel;
        UserProfile = userProfileViewModel;
        ShopDetail = shopDetailViewModel;
        SuggestShop = suggestShopViewModel;
        Settings = settingsViewModel;
        MainTabs = mainTabs;

        FavoritesPlaceholder.Title = Lang.Placeholder_Favorites_Title;
        FavoritesPlaceholder.Message = Lang.Placeholder_Favorites_Message;
        NearMePlaceholder.Title = Lang.Placeholder_NearMe_Title;
        NearMePlaceholder.Message = Lang.Placeholder_NearMe_Message;

        _ = ShopPage.InitializeAsync();

        MainTabs.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(MainWorkspaceSectionCoordinator.SelectedIndex))
                return;

            shellNavigator.CloseUserProfile();
            RefreshBrowseVisibility();
        };

        shellNavigator.AttachProfile(
            id =>
            {
                IsShopDetailOpen = false;
                IsSuggestShopOpen = false;
                IsSettingsOpen = false;
                IsProfileOpen = true;
                _ = UserProfile.LoadAsync(id);
            },
            () =>
            {
                IsProfileOpen = false;
                RefreshBrowseVisibility();
            });

        shellNavigator.AttachShopDetail(
            shopId =>
            {
                IsProfileOpen = false;
                IsSuggestShopOpen = false;
                IsSettingsOpen = false;
                IsShopDetailOpen = true;
                RefreshBrowseVisibility();
                _ = ShopDetail.LoadAsync(shopId);
            },
            () =>
            {
                IsShopDetailOpen = false;
                RefreshBrowseVisibility();
            });

        shellNavigator.AttachSuggestShop(
            () =>
            {
                IsProfileOpen = false;
                IsShopDetailOpen = false;
                IsSettingsOpen = false;
                IsSuggestShopOpen = true;
                RefreshBrowseVisibility();
                _ = SuggestShop.InitializeAsync();
            },
            () =>
            {
                IsSuggestShopOpen = false;
                RefreshBrowseVisibility();
            });

        shellNavigator.AttachSettings(
            () =>
            {
                IsProfileOpen = false;
                IsShopDetailOpen = false;
                IsSuggestShopOpen = false;
                IsSettingsOpen = true;
                RefreshBrowseVisibility();
                _ = Settings.LoadAsync();
            },
            () =>
            {
                IsSettingsOpen = false;
                RefreshBrowseVisibility();
            });
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
}
