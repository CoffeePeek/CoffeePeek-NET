using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class DatabaseModule
{
    extension(IServiceCollection services)
    {
        public PostgresCpOptions GetDatabaseOptions(IConfiguration configuration,
            string databaseName,
            string? connectionStringOverride = null)
        {
            var connectionString =
                connectionStringOverride
                ?? configuration.GetConnectionString(databaseName)
                ?? DatabaseConnectionHelper.GetDatabaseConnectionString();

            var dbOptions = services.AddValidateOptions<PostgresCpOptions>();

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                dbOptions.ConnectionString = connectionString;
            }

            return dbOptions;
        }
    }
}

