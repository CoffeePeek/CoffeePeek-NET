using System.Reflection;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Extentions;
using CoffeePeek.Shared.Persistence;
using CoffeePeek.Shared.Persistence.Data;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeePeek.Shops.Application.Features.CheckIn;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Application.Features.Review;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.Repositories;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using CoffeePeek.Shops.Persistance.Queries;
using CoffeePeek.Shops.Persistance.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoffeePeek.Shops.Persistance;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.AddNpgsqlDbContext<ShopsDbContext>(
                connectionName: AppResources.ShopsDb,
                configureDbContextOptions: opt => opt.AddInterceptors(new AuditInterceptor()),
                configureSettings: settings => { settings.DisableRetry = true; }
            );
        }
        else
        {
            var connectionString = services.AddValidateOptions<PostgresCpOptions>().ConnectionString;

            services.AddDatabase<ShopsDbContext>(
                connectionString,
                opt => opt.AddInterceptors(new AuditInterceptor())
            );
        }

        services.AddScoped<IUnitOfWork, UnitOfWork<ShopsDbContext>>();
        
        
        // Queries
        services.AddScoped<ICheckInQueries, CheckInQueries>();
        services.AddScoped<ICoffeeShopQueries, CoffeeShopQueries>();
        services.AddScoped<IReviewQueries, ReviewQueries>();
        
        // Repositories
        services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        
        // Query Repository 
        services.AddScoped<IQueryCoffeeShopRepository, QueryCoffeeShopRepository>();
        services.AddScoped<ICoffeeShopRepository, CoffeeShopRepository>();
        services.AddScoped<IAdminCoffeeShopQueryRepository, AdminCoffeeShopQueryRepository>();
        services.AddScoped<IQueryReviewRepository, QueryReviewRepository>();
        services.AddScoped<IQueryCheckInRepository, QueryCheckInRepository>();
        services.AddScoped<IQueryCityRepository, QueryCityRepository>();
        services.AddScoped<IQueryEquipmentRepository, QueryEquipmentRepository>();
        services.AddScoped<IQueryRoasterRepository, QueryRoasterRepository>();
        services.AddScoped<IQueryCoffeeBeanRepository, QueryCoffeeBeanRepository>();
        services.AddScoped<IQueryBrewMethodRepository, QueryBrewMethodRepository>();
        services.AddScoped<IAdminStatsQueryRepository, AdminStatsQueryRepository>();
        services.AddScoped<ICommunityFeedQueries, PublicFeedQueryRepository>();
        services.AddCacheModule();

        return services;
    }
}

