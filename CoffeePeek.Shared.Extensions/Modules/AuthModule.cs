using CoffeePeek.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class AuthModule
{
    /// <summary>
    /// Adds user context service that reads user info from headers (set by Gateway)
    /// Use this in downstream services that receive requests through the Gateway
    /// <summary>
    /// Регистрирует доступ к HttpContext и scoped-реализацию IUserContext через HeaderUserContext в контейнере зависимостей.
    /// </summary>
    /// <param name="services">Коллекция сервисов для конфигурации зависимостей.</param>
    /// <returns>Тот же экземпляр <see cref="IServiceCollection"/>, позволяющий продолжить цепочку вызовов конфигурации.</returns>
    public static IServiceCollection AddHeaderUserContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HeaderUserContext>();
        return services;
    }
}
