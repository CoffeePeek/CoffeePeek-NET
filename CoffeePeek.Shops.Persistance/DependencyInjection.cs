using CoffeePeek.Shops.Domain.Aggregates.BrewMethods;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Infrastructure.Configuration;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Extentions;
using CoffeePeek.Shared.Persistence;
using CoffeePeek.Shared.Persistence.Data;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeePeek.Shared.Persistence.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CheckIn = CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate.CheckIn;
using Review = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;

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
        else
        {
            return services.AddValidateOptions<PostgresCpOptions>().ConnectionString;
        }
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, WebApplicationBuilder builder)
    {
        // Database
        string connectionString = GetConnectionString(configuration, services);
        
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

        // Cache
        services.AddCacheModule();

        // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork<ShopsDbContext>>();

        // CAP Module
        services.AddCapModule(connectionString, AppResources.ShopsService);

        // Generic Repositories
        services.AddGenericRepository<CoffeeShop, ShopsDbContext>();
        services.AddGenericRepository<ShopPhoto, ShopsDbContext>();
        services.AddGenericRepository<City, ShopsDbContext>();
        services.AddGenericRepository<Review, ShopsDbContext>();
        services.AddGenericRepository<UserFavorite, ShopsDbContext>();
        services.AddGenericRepository<CoffeeBean, ShopsDbContext>();
        services.AddGenericRepository<Equipment, ShopsDbContext>();
        services.AddGenericRepository<BrewMethod, ShopsDbContext>();
        services.AddGenericRepository<Roaster, ShopsDbContext>();
        services.AddGenericRepository<CheckIn, ShopsDbContext>();

        return services;
    }
}

