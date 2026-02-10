using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Features.Auth.OAuthLogin;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Account.Infrastructure.EventConsumer;
using CoffeePeek.Account.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace CoffeePeek.Account.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddValidateOptions<JWTOptions>();
        
        // 1. Domain Services (реализации интерфейсов из Domain)
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJWTTokenService, JWTTokenService>();
        services.AddScoped<IExternalAuthService, ExternalAuthService>();

        // 2. Event Handlers
        services.AddScoped<CheckinCreatedHandler>();
        services.AddScoped<ReviewAddedHandler>();
        services.AddScoped<ModerationShopApprovedAccountHandler>();
        services.AddScoped<UserPhotoUploadedHandler>();

        // 3 Email Service
        services.AddHttpClient<ResendClient>();
        var resendOptions = services.AddValidateOptions<ResendClientOptions>();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = resendOptions.ApiToken;
        });
        services.AddTransient<IResend, ResendClient>();

        // 4. OAuth
        services.AddValidateOptions<OAuthGoogleOptions>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // 5. Cache (должен быть зарегистрирован до декораторов, использующих Redis)
        services.AddCacheModule();

        return services;
    }
}