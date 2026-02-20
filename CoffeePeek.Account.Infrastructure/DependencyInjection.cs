using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Account.Infrastructure.EventConsumer;
using CoffeePeek.Account.Infrastructure.Identity;
using CoffeePeek.Shared.Auth.Options;
using CoffeePeek.Shared.Kernel.Extentions;
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

        // 2. Event Handlers
        services.AddScoped<CheckinCreatedConsumer>();
        services.AddScoped<ReviewAddedConsumer>();
        services.AddScoped<ModerationShopApprovedAccountConsumer>();
        services.AddScoped<UserPhotoUploadedConsumer>();

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

        return services;
    }
}