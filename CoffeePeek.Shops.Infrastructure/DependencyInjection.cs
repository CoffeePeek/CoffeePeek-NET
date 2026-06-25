using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Application.Abstractions;
using CoffeePeek.Shops.Infrastructure.Account;
using CoffeePeek.Shops.Infrastructure.Consumers;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped<IUserExistenceLookup, AccountUserExistenceLookup>();
        services.AddScoped<ModerationShopApproveHandler>();

        return services;
    }
}
