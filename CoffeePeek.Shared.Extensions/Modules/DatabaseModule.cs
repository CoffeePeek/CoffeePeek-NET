using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class DatabaseModule
{
    public static IServiceCollection AddDatabaseModule<TDbContext>(
        this IServiceCollection services,
        string? connectionStringOverride = null)
        where TDbContext : class
    {
        var connectionString = connectionStringOverride ?? DatabaseConnectionHelper.GetDatabaseConnectionString();
        var dbOptions = services.AddValidateOptions<PostgresCpOptions>();
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            dbOptions.ConnectionString = connectionString;
        }
        
        // Note: AddEfCoreData should be called separately as it's in CoffeePeek.Data.Extensions
        // This module only prepares the connection string and options
        
        return services;
    }
    
    public static PostgresCpOptions GetDatabaseOptions(this IServiceCollection services, string? connectionStringOverride = null)
    {
        var connectionString = connectionStringOverride ?? DatabaseConnectionHelper.GetDatabaseConnectionString();
        var dbOptions = services.AddValidateOptions<PostgresCpOptions>();
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            dbOptions.ConnectionString = connectionString;
        }
        
        return dbOptions;
    }
}

