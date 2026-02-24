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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoffeeShop.Moderation.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, WebApplicationBuilder builder)
    {
#if DEBUG
        builder.AddNpgsqlDbContext<ModerationDbContext>(
            connectionName: AppResources.ModerationDb,
            configureDbContextOptions: opt => opt.AddInterceptors(new AuditInterceptor()),
            configureSettings: settings => { settings.DisableRetry = true; }
        );
    
        services.AddScoped<IUnitOfWork, UnitOfWork<ModerationDbContext>>();
#else
        var connectionString = GetConnectionString(services);

        services.AddDatabase<ModerationDbContext>(
            connectionString,
            opt => opt.AddInterceptors(new AuditInterceptor())
        );
        
        static string GetConnectionString(IServiceCollection services)
        {
            return services.AddValidateOptions<PostgresCpOptions>().ConnectionString;
        }
#endif
        
        services.AddScoped<IQueryModerationReviewRepository, QueryModerationReviewRepository>();
        services.AddScoped<IQueryModerationShopRepository, QueryModerationShopRepository>();
        services.AddScoped<IModerationShopRepository, ModerationShopRepository>();
        services.AddScoped<IModerationReviewRepository, ModerationReviewRepository>();
        
        return services;
    }
}
