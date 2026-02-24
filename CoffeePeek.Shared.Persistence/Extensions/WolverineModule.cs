using System.Reflection;
using CoffeePeek.Shared.Kernel.Extentions;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace CoffeePeek.Shared.Persistence.Extensions;

public static class WolverineModule
{
    extension(WebApplicationBuilder builder)
    {
        public void AddWolverine(Assembly handlerAssembly)
        {
            var rabbitMqOptions = builder.Services.AddValidateOptions<RabbitMqOptions>();
            var postgresCpOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();
            
            builder.Host.UseWolverine(opts =>
            {
                #if DEBUG
                opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;
                #endif
                
                opts.UseRabbitMq(o =>
                    {
                        o.VirtualHost = rabbitMqOptions.VirtualHost;
                        o.Password = rabbitMqOptions.Password;
                        o.UserName = rabbitMqOptions.Username;
                        o.HostName = rabbitMqOptions.HostName;
                        o.Port = rabbitMqOptions.Port;
                    })
                    .AutoProvision();

                opts.PersistMessagesWithPostgresql(postgresCpOptions.ConnectionString);
                opts.UseEntityFrameworkCoreTransactions();
                opts.Discovery.IncludeAssembly(handlerAssembly);

                opts.Policies.AutoApplyTransactions();
            });
        }
    }
}