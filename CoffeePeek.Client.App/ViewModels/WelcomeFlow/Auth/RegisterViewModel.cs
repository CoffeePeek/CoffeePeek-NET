using System.Net.Mail;
using Avalonia;
using Avalonia.Styling;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Res = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;

public enum RegistrationStep
{
    Email,
    Details
}

public sealed partial class RegisterViewModel(
    INavigationService navigation,
    IWebUsersClient usersClient,
    IThemeController themeController) : ViewModelBase
{
    [ObservableProperty]
    public partial RegistrationStep Step { get; set; } = RegistrationStep.Email;

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

    [ObservableProperty]
    public partial bool AgreeToPrivacy { get; set; }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool ThemeShowsSunIcon =>
        Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));

    public bool IsEmailStep => Step == RegistrationStep.Email;

    public bool IsDetailsStep => Step == RegistrationStep.Details;

    partial void OnStepChanged(RegistrationStep value)
    {
        OnPropertyChanged(nameof(IsEmailStep));
        OnPropertyChanged(nameof(IsDetailsStep));
    }

    /// <summary>Called when opening registration from welcome or login.</summary>
    public void ResetForEmailStep()
    {
        Step = RegistrationStep.Email;
        UserName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        ErrorMessage = null;
        AgreeToPrivacy = false;
    }

    [RelayCommand]
    private void GoBack()
    {
        if (Step == RegistrationStep.Details)
        {
            Step = RegistrationStep.Email;
            ErrorMessage = null;
            return;
        }

        navigation.NavigateTo<WelcomePageViewModel>();
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        themeController.ToggleLightDark();
        OnPropertyChanged(nameof(ThemeShowsSunIcon));
    }

    [RelayCommand]
    private void GoToEmailStep()
    {
        Step = RegistrationStep.Email;
        ErrorMessage = null;
    }

    [RelayCommand]
    private void GoToLogin() =>
        navigation.NavigateTo<LoginViewModel>(l =>
        {
            l.Email = Email.Trim();
            l.Password = string.Empty;
            l.ErrorMessage = null;
        });

    [RelayCommand]
    private async Task CheckEmailAsync()
    {
        ErrorMessage = null;
        var trimmed = Email.Trim();
        if (string.IsNullOrEmpty(trimmed) || !IsValidEmail(trimmed))
        {
            ErrorMessage = Res.Auth_InvalidEmail;
            return;
        }

        var check = await usersClient.EmailIsRegisteredAsync(trimmed);
        if (check.IsFailed)
        {
            ErrorMessage = check.Errors.FirstOrDefault()?.Message ?? Res.Auth_EmailCheckFailed;
            return;
        }

        if (check.Value)
        {
            navigation.NavigateTo<LoginViewModel>(l =>
            {
                l.Email = trimmed;
                l.Password = string.Empty;
                l.ErrorMessage = null;
            });
            return;
        }

        Email = trimmed;
        UserName = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        AgreeToPrivacy = false;
        Step = RegistrationStep.Details;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;

        if (!AgreeToPrivacy)
        {
            ErrorMessage = Res.Auth_PrivacyRequired;
            return;
        }

        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Email)
                                                 || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = Res.Auth_FillAllFields;
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = Res.Auth_PasswordMinLength;
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = Res.Auth_PasswordMismatch;
            return;
        }

        // await register API when wired
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
