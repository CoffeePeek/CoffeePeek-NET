using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.Home;

public sealed partial class HomeViewModel(IAccountSignOutService accountSignOutService) : ViewModelBase
{
    [RelayCommand]
    private async Task SignOutAsync() => await accountSignOutService.SignOutAsync();
}
