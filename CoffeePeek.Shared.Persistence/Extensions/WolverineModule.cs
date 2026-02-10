using System.Reflection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace CoffeePeek.Shared.Persistence.Extensions;

public static class WolverineModule
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder AddWolverine(string connectionString)
        {
            hostBuilder.UseWolverine(opts =>
            {
                opts.PersistMessagesWithPostgresql(connectionString);
                
                opts.UseEntityFrameworkCoreTransactions();
            });

            return hostBuilder;
        }
    }
}