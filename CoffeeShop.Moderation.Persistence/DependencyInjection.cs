using System.Reflection;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Extentions;
using CoffeePeek.Shared.Persistence;
using CoffeePeek.Shared.Persistence.Data;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeeShop.Moderation.Persistence.Configuration;
using CoffeeShop.Moderation.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoffeeShop.Moderation.Persistence;

public static class DependencyInjection
{
    public static string GetConnectionString(IConfiguration configuration, IServiceCollection services)
    {
        if (configuration["DOTNET_ASPIRE"] == "true")
        {
            return configuration.GetConnectionString(AppResources.ModerationDb) ?? 
                   throw new InvalidOperationException("Connection string not found");
        }
        else
        {
            return services.AddValidateOptions<PostgresCpOptions>().ConnectionString;
        }
    }
    
    public static IHostBuilder AddPersistence(this IHostBuilder hostBuilder, Assembly handlersAssembly, string connectionString)
    {
        hostBuilder.AddWolverine(handlersAssembly, connectionString);
        
        return hostBuilder;
    }
    
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, WebApplicationBuilder builder)
    {
        var connectionString = GetConnectionString(configuration, services);
        
        if (configuration["DOTNET_ASPIRE"] == "true")
        {
            builder.AddNpgsqlDbContext<ModerationDbContext>(
                connectionName: AppResources.ModerationDb, 
                configureDbContextOptions: opt => opt.AddInterceptors(new AuditInterceptor()),
                configureSettings: settings => { settings.DisableRetry = true; }
            );
        }
        else
        {
            services.AddDbContext<ModerationDbContext>(opt => opt.UseNpgsql(connectionString).AddInterceptors(new AuditInterceptor()));
        }

        services.AddScoped<IQueryModerationReviewRepository, QueryModerationReviewRepository>();
        services.AddScoped<IQueryModerationShopRepository, QueryModerationShopRepository>();
        services.AddScoped<IModerationShopRepository, ModerationShopRepository>();
        services.AddScoped<IModerationReviewRepository, ModerationReviewRepository>();
        
        return services;
    }
}
