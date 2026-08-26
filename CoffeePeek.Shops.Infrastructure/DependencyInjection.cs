using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Options;
using CoffeePeek.Shops.Application.Abstractions;
using CoffeePeek.Shops.Infrastructure.Account;
using CoffeePeek.Shops.Infrastructure.Consumers;
using CoffeePeek.Shops.Infrastructure.Menu;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Shops.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient("account-user-lookup", client =>
        {
            client.BaseAddress = new Uri($"http://{AppResources.AccountService}");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddHttpClient("gemini", (sp, client) =>
        {
            var timeout = sp.GetRequiredService<IOptions<GeminiOptions>>().Value.TimeoutSeconds;
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(timeout, 5, 180));
        });

        services.AddHttpClient("menu-photos", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        services.AddScoped<IUserExistenceLookup, AccountUserExistenceLookup>();
        services.AddScoped<IMenuVisionParser, GeminiMenuVisionParser>();
        services.AddScoped<IMenuPhotoDownloader, HttpMenuPhotoDownloader>();
        services.AddScoped<ModerationShopApproveHandler>();

        return services;
    }
}
