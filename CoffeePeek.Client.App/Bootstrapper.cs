using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.ViewModels;
using CoffeePeek.Client.App.ViewModels.Home;
using CoffeePeek.Client.App.ViewModels.WelcomeFlow.Auth;

namespace CoffeePeek.Client.App;

public static class Bootstrapper
{
    public static IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();

        builder.Register(_ =>
        {
            var handler = new HttpClientHandler { UseCookies = true };
            var baseUrl = ClientAppDefaults.ApiBaseUrl.TrimEnd('/') + "/";
            return new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
        }).SingleInstance();

        builder.RegisterType<ClientSession>().As<IClientSession>().SingleInstance();
        builder.RegisterType<AccountApi>().As<IAccountApi>().SingleInstance();

        builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();
        builder.Register(c => c.Resolve<MainViewModel>().Navigation).As<IAuthNavigation>().SingleInstance();

        builder.RegisterType<LoginViewModel>().AsSelf();
        builder.RegisterType<RegisterEmailViewModel>().AsSelf();
        builder.RegisterType<RegisterViewModel>().AsSelf();
        builder.RegisterType<HomeViewModel>().AsSelf();

        return builder.Build();
    }
}
