using Autofac;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Home;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;

namespace CoffeePeek.Client.App.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly ILifetimeScope _scope;
    private ViewModelBase? _currentPage;

    public MainViewModel(ILifetimeScope scope)
    {
        _scope = scope;
        Navigation = new AuthNavigationService(this, scope);
        CurrentPage = new WelcomePageViewModel(Navigation);
    }

    public IAuthNavigation Navigation { get; }

    public ViewModelBase? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    private sealed class AuthNavigationService(MainViewModel main, ILifetimeScope scope) : IAuthNavigation
    {
        public void ShowWelcome() => main.CurrentPage = new WelcomePageViewModel(main.Navigation);

        public void ShowLogin(string? email = null)
        {
            var vm = scope.Resolve<LoginViewModel>();
            if (!string.IsNullOrWhiteSpace(email))
                vm.Email = email.Trim();
            main.CurrentPage = vm;
        }

        public void ShowRegisterEmail() => main.CurrentPage = scope.Resolve<RegisterEmailViewModel>();

        public void ShowRegister(string email)
        {
            var vm = scope.Resolve<RegisterViewModel>();
            vm.Email = email.Trim();
            main.CurrentPage = vm;
        }

        public void ShowHome() => main.CurrentPage = scope.Resolve<HomeViewModel>();
    }
}
