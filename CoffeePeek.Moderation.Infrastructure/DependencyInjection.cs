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
            // API key in header — never appears in request URLs or logs
            client.DefaultRequestHeaders.Add("X-Yandex-API-Key", yandexOptions.ApiKey);
        });
        
        return services;
    }
}