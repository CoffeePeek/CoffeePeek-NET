using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Features.Auth.OAuthLogin;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Account.Infrastructure.EventConsumer;
using CoffeePeek.Account.Infrastructure.Identity;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Infrastructure.Abstract.S3;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Resend;

namespace CoffeePeek.Account.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // 1. Domain Services (реализации интерфейсов из Domain)
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJWTTokenService, JWTTokenService>();
        services.AddScoped<IExternalAuthService, ExternalAuthService>();

        // 5. External Services
        services.AddScoped<IStorageService, MinIOStorageService>();
        var minIoOptions = services.AddValidateOptions<MinIOOptions>();
        services
            .AddMinio(configureClient =>
                configureClient
                    .WithEndpoint(new Uri(minIoOptions.Endpoint))
                    .WithCredentials(minIoOptions.AccessKey, minIoOptions.SecretKey)
                    .Build()
            );

        // 3. Event Handlers
        services.AddScoped<CheckinCreatedHandler>();
        services.AddScoped<ReviewAddedHandler>();
        services.AddScoped<ModerationShopApprovedAccountHandler>();

        // 4. Email Service
        services.AddHttpClient<ResendClient>();
        var resendOptions = services.AddValidateOptions<ResendClientOptions>();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = resendOptions.ApiToken;
        });
        services.AddTransient<IResend, ResendClient>();

        // 5. OAuth
        services.AddValidateOptions<OAuthGoogleOptions>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // 6. Cache (должен быть зарегистрирован до декораторов, использующих Redis)
        services.AddCacheModule();

        return services;
    }
}