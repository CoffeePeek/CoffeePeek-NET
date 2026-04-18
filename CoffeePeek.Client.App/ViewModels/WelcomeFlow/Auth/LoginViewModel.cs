using Res = CoffeePeek.Client.App.Resources.Lang.Resources;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;

public sealed partial class LoginViewModel(INavigationService navigationService) : ViewModelBase
{
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [RelayCommand]
    private void GoBack() => navigationService.NavigateTo<WelcomePageViewModel>();

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = Res.Auth_FillAllFields;
            return;
        }
        //
        // var result = await accountApi.LoginAsync(Email.Trim(), Password);
        // if (!result.Ok || string.IsNullOrEmpty(result.Value))
        // {
        //     ErrorMessage = result.ErrorMessage ?? Res.Auth_LoginFailed;
        //     return;
        // }
        //
        // session.SetAccessToken(result.Value);
        // Password = string.Empty;
        // navigation.ShowHome();
    }
}
