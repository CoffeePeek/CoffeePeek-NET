using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Shops;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class WorkspaceViewModel : ViewModelBase
{
    public ShopsPageViewModel ShopPage { get; }

    public UserProfileViewModel UserProfile { get; }

    [ObservableProperty]
    public partial bool IsProfileOpen { get; private set; }

    public WorkspaceViewModel(
        ShopsPageViewModel shopsPageViewModel,
        UserProfileViewModel userProfileViewModel,
        WorkspaceShellNavigator shellNavigator)
    {
        ShopPage = shopsPageViewModel;
        UserProfile = userProfileViewModel;
        _ = ShopPage.InitializeAsync();
        shellNavigator.Attach(
            id =>
            {
                IsProfileOpen = true;
                _ = UserProfile.LoadAsync(id);
            },
            () => IsProfileOpen = false);
    }
}
