using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Auth.Extensions;

public static class AuthModule
{
    public const string HeaderAuth = nameof(HeaderAuth);
    
    /// <summary>
    /// Adds user context service that reads user info from headers (set by Gateway)
    /// Use this in downstream services that receive requests through the Gateway
    /// </summary>
    public static IServiceCollection AddHeaderUserContext(this IServiceCollection services)
    {
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