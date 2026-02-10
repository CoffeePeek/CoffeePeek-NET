using CoffeePeek.Shops.Application.Common;
using CoffeePeek.Shops.Infrastructure.Consumers;
using CoffeePeek.Shops.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shops.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Custom Repositories
        services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
        services.AddScoped<ICoffeeShopRepository, CoffeeShopRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IUserCheckInRepository, UserCheckInRepository>();

        // Cache Services
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ICoffeeShopCacheService, CoffeeShopCacheService>();

        // CAP Handlers
        services.AddScoped<ModerationShopApprovedHandler>();
        services.AddScoped<ModerationReviewApprovedHandler>();
        services.AddScoped<UserNameChangedHandler>();

        return services;
    }
}

