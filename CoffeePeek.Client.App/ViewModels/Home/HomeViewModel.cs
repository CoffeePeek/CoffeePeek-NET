using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.Home;

public sealed partial class HomeViewModel(
    IAuthNavigation navigation,
    IClientSession session) : ViewModelBase
{
    [RelayCommand]
    private void SignOut()
    {
        session.Clear();
        navigation.ShowWelcome();
    }
}
