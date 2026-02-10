using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Mapper;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shops.Infrastructure.Extensions;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shops.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatRModule(typeof(GetCoffeeShopHandler));

        // Mapster
        services.AddMapster();
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(MapsterConfiguration).Assembly);

        // Validation
        services.AddValidators();

        // Application Services
        services.AddScoped<ICreateShopFromModerationService, CreateShopFromModerationService>();
        services.AddScoped<IUserFavoriteService, UserFavoriteService>();

        return services;
    }
}

