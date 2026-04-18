using System.Threading.Tasks;
using CoffeePeek.Client.App.Core.Cache;
using Res = CoffeePeek.Client.App.Resources.Lang.Resources;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;

public sealed partial class RegisterViewModel(
    INavigationService navigation,
    IClientSession session) : ViewModelBase
{
    [ObservableProperty]
    public partial string UserName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [RelayCommand]
    private void GoBack() => navigation.NavigateTo<RegisterViewModel>();

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Email)
                                                 || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = Res.Auth_FillAllFields;
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = Res.Auth_PasswordMismatch;
            return;
        }

        // var reg = await accountApi.RegisterAsync(UserName.Trim(), Email.Trim(), Password);
        // if (!reg.Ok)
        // {
        //     ErrorMessage = reg.ErrorMessage ?? Res.Auth_RegisterFailed;
        //     return;
        // }

        // var login = await accountApi.LoginAsync(Email.Trim(), Password);
        // if (!login.Ok || string.IsNullOrEmpty(login.Value))
        // {
        //     ErrorMessage = login.ErrorMessage ?? Res.Auth_LoginAfterRegisterFailed;
        //     Password = string.Empty;
        //     ConfirmPassword = string.Empty;
        //     navigation.ShowLogin(Email);
        //     return;
        // }
        //
        // session.SetAccessToken(login.Value);
        // Password = string.Empty;
        // ConfirmPassword = string.Empty;
        // navigation.ShowHome();
    }
}
