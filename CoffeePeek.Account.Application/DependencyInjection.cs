using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Features.Auth.Login;
using CoffeePeek.Account.Application.Features.Auth.OAuthLogin;
using CoffeePeek.Account.Application.Mapper;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Extensions.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Account.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Mapster
        services.AddSingleton(MapsterConfiguration.CreateMapper());
        
        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton<EmailExistenceFilter>(_ =>
        {
            const int expectedCount = 1000000;
            const double errorRate = 0.01;

            return new EmailExistenceFilter(expectedCount, errorRate);
        });

        services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
        
        
        services.AddMediatRModule(typeof(LoginUserHandler));
        
        services.AddScoped<IExternalAuthService, ExternalAuthService>();
        
        return services;
    }
}