using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeePeek.Client.App.ViewModels.Auth;

public sealed partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Email { get; set; }
    
    [ObservableProperty]
    public partial string Password { get; set; }

    [RelayCommand]
    public async Task Login()
    {
    }
}