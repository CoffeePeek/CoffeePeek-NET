using System.Net.Mail;
using System.Threading.Tasks;
using Res = CoffeePeek.Client.App.Resources.Lang.Resources;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;

public sealed partial class RegisterEmailViewModel(INavigationService navigation) : ViewModelBase
{
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [RelayCommand]
    private void GoBack() => navigation.NavigateTo<WelcomePageViewModel>();

    [RelayCommand]
    private async Task ContinueAsync()
    {
        ErrorMessage = null;
        var trimmed = Email.Trim();
        if (string.IsNullOrEmpty(trimmed) || !IsValidEmail(trimmed))
        {
            ErrorMessage = Res.Auth_InvalidEmail;
            return;
        }

        // var availability = await accountApi.CheckEmailAsync(trimmed);
        // switch (availability)
        // {
        //     case EmailAvailability.Exists:
        //         navigation.ShowLogin(trimmed);
        //         break;
        //     case EmailAvailability.Available:
        //         navigation.ShowRegister(trimmed);
        //         break;
        //     default:
        //         ErrorMessage = Res.Auth_EmailCheckFailed;
        //         break;
        // }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
