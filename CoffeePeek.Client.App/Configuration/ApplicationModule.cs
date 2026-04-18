using Autofac;
using CoffeePeek.Client.App.ViewModels;
using CoffeePeek.Client.App.ViewModels.Home;
using CoffeePeek.Client.App.ViewModels.Shops;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;

namespace CoffeePeek.Client.App.Configuration;

public class ApplicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<HeaderViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<WorkspaceViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<HomeViewModel>().AsSelf().SingleInstance();
        
        RegisterWelcomeFlow(builder);
        RegisterShopPage(builder);
        
        builder.RegisterModule<UiServiceModule>();
        
    }
    
    private static void RegisterWelcomeFlow(ContainerBuilder builder)
    {
        builder.RegisterType<LoginViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<RegisterEmailViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<RegisterViewModel>().AsSelf().SingleInstance();
    }
    
    private static void RegisterShopPage(ContainerBuilder builder)
    {
        builder.RegisterType<ShopsPageViewModel>().AsSelf().SingleInstance();
    }
}