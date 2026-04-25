using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Shops;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class WorkspaceViewModel : ViewModelBase
{
    public ShopsPageViewModel ShopPage { get; }

    public UserProfileViewModel UserProfile { get; }

    public ShopDetailViewModel ShopDetail { get; }

    public SuggestShopViewModel SuggestShop { get; }

    [ObservableProperty]
    public partial bool IsProfileOpen { get; private set; }

    [ObservableProperty]
    public partial bool IsShopDetailOpen { get; private set; }

    [ObservableProperty]
    public partial bool IsSuggestShopOpen { get; private set; }

    public bool IsShopsListVisible => !IsProfileOpen && !IsShopDetailOpen && !IsSuggestShopOpen;

    public WorkspaceViewModel(
        ShopsPageViewModel shopsPageViewModel,
        UserProfileViewModel userProfileViewModel,
        ShopDetailViewModel shopDetailViewModel,
        SuggestShopViewModel suggestShopViewModel,
        WorkspaceShellNavigator shellNavigator)
    {
        ShopPage = shopsPageViewModel;
        UserProfile = userProfileViewModel;
        ShopDetail = shopDetailViewModel;
        SuggestShop = suggestShopViewModel;
        _ = ShopPage.InitializeAsync();

        shellNavigator.AttachProfile(
            id =>
            {
                IsShopDetailOpen = false;
                IsSuggestShopOpen = false;
                IsProfileOpen = true;
                _ = UserProfile.LoadAsync(id);
            },
            () =>
            {
                IsProfileOpen = false;
                OnPropertyChanged(nameof(IsShopsListVisible));
            });

        shellNavigator.AttachShopDetail(
            shopId =>
            {
                IsProfileOpen = false;
                IsSuggestShopOpen = false;
                IsShopDetailOpen = true;
                OnPropertyChanged(nameof(IsShopsListVisible));
                _ = ShopDetail.LoadAsync(shopId);
            },
            () =>
            {
                IsShopDetailOpen = false;
                OnPropertyChanged(nameof(IsShopsListVisible));
            });

        shellNavigator.AttachSuggestShop(
            () =>
            {
                IsProfileOpen = false;
                IsShopDetailOpen = false;
                IsSuggestShopOpen = true;
                OnPropertyChanged(nameof(IsShopsListVisible));
                _ = SuggestShop.InitializeAsync();
            },
            () =>
            {
                IsSuggestShopOpen = false;
                OnPropertyChanged(nameof(IsShopsListVisible));
            });
    }

    partial void OnIsProfileOpenChanged(bool value) => OnPropertyChanged(nameof(IsShopsListVisible));

    partial void OnIsShopDetailOpenChanged(bool value) => OnPropertyChanged(nameof(IsShopsListVisible));

    partial void OnIsSuggestShopOpenChanged(bool value) => OnPropertyChanged(nameof(IsShopsListVisible));
}
