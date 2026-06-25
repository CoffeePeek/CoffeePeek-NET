using CoffeePeek.Shared.Auth.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Auth.Extensions;

public static class AuthModule
{
    public const string HeaderAuth = nameof(HeaderAuth);

    /// <summary>
    /// Adds user context service that reads user info from gateway-trusted headers.
    /// </summary>
    public static IServiceCollection AddHeaderUserContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<GatewayAuthOptions>()
            .Bind(configuration.GetSection(GatewayAuthOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = HeaderAuth;
                options.DefaultChallengeScheme = HeaderAuth;
            })
            .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>(HeaderAuth, null);

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HeaderUserContext>();
        return services;
    }
}
