using Autofac;
using CoffeePeek.Client.App.ViewModels;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;
using LoginViewModel = CoffeePeek.Client.App.ViewModels.Auth.LoginViewModel;

namespace CoffeePeek.Client.App;

public static class Bootstrapper
{
    public static IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<LoginViewModel>().AsSelf();
        builder.RegisterType<WelcomePageViewModel>().AsSelf();
        builder.RegisterType<WelcomePageCardItemViewModel>().AsSelf();
        
        return builder.Build();
    }
}