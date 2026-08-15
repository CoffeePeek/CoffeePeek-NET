using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Infrastructure.Services;
using CoffeePeek.Shared.Kernel.Extentions;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Moderation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var yandexOptions = services.AddValidateOptions<YandexApiOptions>();
        services.AddHttpClient<IYandexGeocodingService, YandexGeocodingService>(client =>
        {
            client.BaseAddress = new Uri(yandexOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(yandexOptions.TimeoutSeconds);
        });

        services.AddOptions<GooglePlaces>().BindConfiguration(nameof(GooglePlaces));
        services.AddHttpClient<IGooglePlacesLookup, GooglePlacesLookup>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GooglePlaces>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 60));
        });

        services.AddHttpClient<IOverpassClient, OverpassClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(120);
        });
        
        return services;
    }
}