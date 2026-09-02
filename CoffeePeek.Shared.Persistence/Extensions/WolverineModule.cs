using System.Reflection;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Extentions;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
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

                // Every CoffeePeek service is deployed as a single instance (no replicas in
                // deploy/docker-compose.yml or CoffePeek.AppHost). Wolverine's default
                // DurabilityMode.Balanced runs leader election and cross-node agent
                // coordination (AssignAgent/StartAgent) that assumes a multi-node cluster;
                // with only one node that handshake occasionally exceeds its ack timeout on
                // restart and throws a handled TimeoutException for no functional benefit.
                // Solo mode disables that cross-node coordination while still recovering the
                // transactional inbox/outbox on startup.
                opts.Durability.Mode = DurabilityMode.Solo;

                opts.UseEntityFrameworkCoreTransactions();
                foreach (var assembly in handlerAssembly)
                {
                    opts.Discovery.IncludeAssembly(assembly);
                }

                opts.Policies.AutoApplyTransactions();

                opts.Policies
                    .OnException<ConflictException>(ex => ex.InnerException is DbUpdateConcurrencyException)
                    .RetryWithCooldown(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(150), TimeSpan.FromMilliseconds(400));
            });
        }
    }
}