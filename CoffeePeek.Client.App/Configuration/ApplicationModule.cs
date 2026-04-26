using Autofac;
using CoffeePeek.Client.App.Core.Execution;
using CoffeePeek.Client.App.Execution;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.Startup;
using CoffeePeek.Client.App.ViewModels;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Home;
using CoffeePeek.Client.App.ViewModels.Shops;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;

namespace CoffeePeek.Client.App.Configuration;

public class ApplicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c =>
            {
                var scope = c.Resolve<ILifetimeScope>();
                return new NavigationService(type => (ViewModelBase)scope.Resolve(type));
            })
            .As<INavigationService>()
            .SingleInstance();

        builder.RegisterType<ApplicationExecutorRunner>().As<IApplicationExecutorRunner>().SingleInstance();
        builder.RegisterType<RestoreSessionExecutor>().As<IBeforeMainShellExecutor>().SingleInstance();
        builder.RegisterType<ApplyPersistedThemeExecutor>().As<IBeforeMainShellExecutor>().SingleInstance();
        builder.RegisterType<InitialRouteExecutor>().As<IBeforeMainShellExecutor>().SingleInstance();

        builder.RegisterType<WorkspaceShellNavigator>()
            .AsSelf()
            .As<IWorkspaceShellNavigator>()
            .SingleInstance();

        builder.RegisterType<MainWorkspaceSectionCoordinator>().AsSelf().SingleInstance();
        builder.RegisterType<AccountSignOutService>().As<IAccountSignOutService>().SingleInstance();

        builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<HeaderViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<UserProfileViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<WorkspaceViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<HomeViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<SettingsViewModel>().AsSelf().SingleInstance();
        
        RegisterWelcomeFlow(builder);
        RegisterShopPage(builder);
        
        builder.RegisterModule<UiServiceModule>();
        
    }
    
    private static void RegisterWelcomeFlow(ContainerBuilder builder)
    {
        builder.RegisterType<ThemeController>().As<IThemeController>().SingleInstance();

        builder.RegisterType<WelcomePageViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<LoginViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<RegisterViewModel>().AsSelf().SingleInstance();
    }
    
    private static void RegisterShopPage(ContainerBuilder builder)
    {
        builder.RegisterType<ShopsPageViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ShopDetailViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<SuggestShopViewModel>().AsSelf().SingleInstance();
    }
}