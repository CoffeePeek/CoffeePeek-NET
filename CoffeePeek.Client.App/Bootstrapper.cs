using Autofac;
using Avalonia;
using Avalonia.Styling;
using CoffeePeek.Client.App.Configuration;
using CoffeePeek.Client.App.Core.Cache;
using CoffeePeek.Client.App.Infrastructure.Cache;
using CoffeePeek.Client.App.Infrastructure.Configuration;
using CoffeePeek.Client.App.Infrastructure.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;

namespace CoffeePeek.Client.App;

public static class Bootstrapper
{
    public static IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();
        var configuration = builder.RegisterClientAppConfiguration();
        builder.RegisterOptionsFromSection<ApiOptions>(configuration);
        builder.RegisterOptionsFromSection<AuthClientOptions>(configuration);
        builder.RegisterOptionsFromSection<LocalUserSettingsOptions>(configuration);

        if (Application.Current is not null)
            Application.Current.RequestedThemeVariant = ThemeVariant.Default;

        builder.RegisterType<ClientSession>().As<IClientSession>().SingleInstance();
        builder.RegisterModule<InfrastructureModule>();
        builder.RegisterModule<ApplicationModule>();

        return builder.Build();
    }
}
