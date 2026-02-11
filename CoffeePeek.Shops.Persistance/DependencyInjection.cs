using System.Reflection;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Extentions;
using CoffeePeek.Shared.Persistence;
using CoffeePeek.Shared.Persistence.Data;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using CoffeePeek.Shops.Persistance.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoffeePeek.Shops.Persistance;

public static class DependencyInjection
{
    public static string GetConnectionString(IConfiguration configuration, IServiceCollection services)
    {
        if (configuration["DOTNET_ASPIRE"] == "true")
        {
            return configuration.GetConnectionString(AppResources.ShopsDb) ?? 
                   throw new InvalidOperationException("Connection string not found");
        }

        return services.AddValidateOptions<PostgresCpOptions>().ConnectionString;
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
            builder.AddNpgsqlDbContext<ShopsDbContext>(
                connectionName: AppResources.ShopsDb, 
                configureDbContextOptions: opt => opt.AddInterceptors(new AuditInterceptor()),
                configureSettings: settings => { settings.DisableRetry = true; }
            );
        }
        else
        {
            services.AddDbContext<ShopsDbContext>(opt => opt.UseNpgsql(connectionString).AddInterceptors(new AuditInterceptor()));
        }

        services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
        services.AddScoped<IQueryCoffeeShopRepository, QueryCoffeeShopRepository>();
        services.AddScoped<IQueryReviewRepository, QueryReviewRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IQueryCheckInRepository, QueryCheckInRepository>();
        services.AddScoped<IQueryCityRepository, QueryCityRepository>();
        services.AddScoped<IQueryEquipmentRepository, QueryEquipmentRepository>();
        services.AddScoped<IQueryRoasterRepository, QueryRoasterRepository>();
        services.AddScoped<IQueryCoffeeBeanRepository, QueryCoffeeBeanRepository>();
        services.AddScoped<IQueryBrewMethodRepository, QueryBrewMethodRepository>();
        
        // Cache
        services.AddCacheModule();

        return services;
    }
}

