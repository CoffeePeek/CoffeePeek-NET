using Autofac;
using CoffeePeek.Client.App.Core.Settings;
using CoffeePeek.Client.App.Infrastructure.LocalSettings;
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
            .Register(_ => LoggerFactory.Create(b =>
            {
                b.SetMinimumLevel(LogLevel.Information);
                b.AddSimpleConsole(o =>
                {
                    o.SingleLine = true;
                    o.TimestampFormat = "HH:mm:ss ";
                });
            }))
            .As<ILoggerFactory>()
            .SingleInstance();

        builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
    }
}
