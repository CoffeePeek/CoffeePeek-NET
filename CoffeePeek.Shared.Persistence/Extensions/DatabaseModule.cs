using CoffeePeek.Shared.Kernel.Extentions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Persistence.Extensions;

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
                ?? GetDatabaseConnectionString();

            var dbOptions = services.AddValidateOptions<PostgresCpOptions>();

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                dbOptions.ConnectionString = connectionString;
            }

            return dbOptions;
        }
    }
    
    private static string? GetDatabaseConnectionString()
    {
        var directConnectionString = Environment.GetEnvironmentVariable("PostgresCpOptions__ConnectionString");
        if (!string.IsNullOrEmpty(directConnectionString))
        {
            return directConnectionString;
        }
        
        return null;
    }
}

