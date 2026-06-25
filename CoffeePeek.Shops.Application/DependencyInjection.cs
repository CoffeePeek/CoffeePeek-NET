using CoffeePeek.Shops.Application.Extensions;
using CoffeePeek.Shops.Application.Mapper;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shared.Kernel.Options;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Shops.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddOptions<MediaPublicUrlOptions>()
            .BindConfiguration(nameof(MediaPublicUrlOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<TypeAdapterConfig>(sp =>
            MapsterConfiguration.CreateConfig(sp.GetRequiredService<IOptions<MediaPublicUrlOptions>>().Value));

        services.AddSingleton<IMapper>(sp => new MapsterMapper.Mapper(sp.GetRequiredService<TypeAdapterConfig>()));

        // Validation
        services.AddValidators();

        // Application Services
        services.AddScoped<ICreateShopFromModerationService, CreateShopFromModerationService>();
        services.AddScoped<IUserFavoriteService, UserFavoriteService>();

        return services;
    }
}
