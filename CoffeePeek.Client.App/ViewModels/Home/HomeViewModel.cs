using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.Home;

public sealed partial class HomeViewModel(IAccountSignOutService accountSignOutService) : ViewModelBase
{
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        var result = await accountSignOutService.SignOutAsync();
        if (result.IsFailed)
            ErrorMessage = result.Errors.FirstOrDefault()?.Message;
    }
}
