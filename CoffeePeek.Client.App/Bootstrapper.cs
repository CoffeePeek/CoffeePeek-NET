using Autofac;
using Avalonia;
using Avalonia.Styling;
using CoffeePeek.Client.App.Configuration;
using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Infrastructure.Cache;

namespace CoffeePeek.Client.App;

public static class Bootstrapper
{
    public static IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();

        Application.Current.RequestedThemeVariant = ThemeVariant.Light;
        builder.RegisterType<ClientSession>().As<IClientSession>().SingleInstance();

        builder.RegisterModule<ApplicationModule>();

        return builder.Build();
    }
}
