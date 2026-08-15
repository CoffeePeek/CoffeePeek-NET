using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Infrastructure.Services;
using CoffeePeek.Shared.Kernel.Extentions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

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

        // Aspire AddStandardResilienceHandler() defaults to 30s total timeout and overrides HttpClient.Timeout.
        // Overpass QL for Minsk is declared as [timeout:90] — replace the default pipeline for this client only.
        var attemptTimeout = TimeSpan.FromSeconds(120);
        services.AddHttpClient<IOverpassClient, OverpassClient>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CoffeePeek/1.0 (https://coffeepeek.by; osm-import)");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        })
        .RemoveAllResilienceHandlers()
        .AddStandardResilienceHandler(options =>
        {
            options.AttemptTimeout.Timeout = attemptTimeout;
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(150);
            options.Retry.MaxRetryAttempts = 0;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(300);
            options.CircuitBreaker.MinimumThroughput = 100;
        });
        
        return services;
    }
}