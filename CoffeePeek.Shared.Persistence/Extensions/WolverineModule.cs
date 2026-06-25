using System.Reflection;
using CoffeePeek.Shared.Kernel.Extentions;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Wolverine.Transports;

namespace CoffeePeek.Shared.Persistence.Extensions;

public static class WolverineModule
{
    extension(WebApplicationBuilder builder)
    {
        public void AddWolverine(Assembly[] handlerAssembly)
        {
            var rabbitMqOptions = builder.Services.AddValidateOptions<RabbitMqOptions>();
            var postgresCpOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();
            
            builder.Host.UseWolverine(opts =>
            {
                opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;

                opts.UseRabbitMq(o =>
                    {
                        o.VirtualHost = string.IsNullOrWhiteSpace(rabbitMqOptions.VirtualHost)
                            ? "/"
                            : rabbitMqOptions.VirtualHost;
                        o.Password = rabbitMqOptions.Password;
                        o.UserName = rabbitMqOptions.Username;
                        o.HostName = rabbitMqOptions.HostName;
                        o.Port = rabbitMqOptions.Port;
                        o.RequestedConnectionTimeout = TimeSpan.FromSeconds(30);
                        o.AutomaticRecoveryEnabled = true;
                        o.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
                    })
                    .AutoProvision()
                    // Each handler gets its own queue (Account + Shops both listen to ModerationShopApprovedEvent).
                    .UseConventionalRouting(NamingSource.FromHandlerType);

                opts.PersistMessagesWithPostgresql(postgresCpOptions.ConnectionString);
                opts.UseEntityFrameworkCoreTransactions();
                foreach (var assembly in handlerAssembly)
                {
                    opts.Discovery.IncludeAssembly(assembly);
                }

                opts.Policies.AutoApplyTransactions();
            });
        }
    }
}