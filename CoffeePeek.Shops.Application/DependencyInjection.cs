using CoffeePeek.Shops.Application.Extensions;
using CoffeePeek.Shops.Application.Mapper;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shops.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Mapster
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(MapsterConfiguration).Assembly);

        services.AddSingleton(config);
        services.AddMapster();

        // Validation
        services.AddValidators();

        // Application Services
        services.AddScoped<ICreateShopFromModerationService, CreateShopFromModerationService>();
        services.AddScoped<IUserFavoriteService, UserFavoriteService>();

        return services;
    }
}

