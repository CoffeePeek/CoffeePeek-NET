using Autofac;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Infrastructure.LocalSettings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Client.App.Infrastructure.Configuration;

public sealed class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterLogging(builder);
        builder.RegisterModule<HttpModule>();
        builder.RegisterType<JsonFileLocalUserSettings>().As<ILocalUserSettings>().SingleInstance();
    }

    private static void RegisterLogging(ContainerBuilder builder)
    {
        builder
            .Register(_ => NullLoggerFactory.Instance)
            .As<ILoggerFactory>()
            .SingleInstance();

        builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
    }
}
