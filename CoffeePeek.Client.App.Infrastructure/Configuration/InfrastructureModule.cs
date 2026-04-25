using Autofac;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Infrastructure.LocalSettings;

namespace CoffeePeek.Client.App.Infrastructure.Configuration;

public sealed class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<HttpModule>();
        builder.RegisterType<JsonFileLocalUserSettings>().As<ILocalUserSettings>().SingleInstance();
    }
}
