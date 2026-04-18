using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Shops;

namespace CoffeePeek.Client.App.ViewModels.Home;

public class WorkspaceViewModel(ShopsPageViewModel shopsPageViewModel) : ViewModelBase
{
    public ShopsPageViewModel ShopPage { get; } = shopsPageViewModel;
}