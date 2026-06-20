using System.Text;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Auth.Options;
using CoffeePeek.Shared.Kernel.Extentions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace CoffeePeek.Gateway.Extensions;

/// <summary>
/// Extension methods for configuring JWT authentication and role-based authorization policies
/// at the Gateway level.
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Registers JWT Bearer authentication and all role-based authorization policies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">
    ///     The host environment — used to disable HTTPS metadata requirement in Development.
    ///     Uses <see cref="IWebHostEnvironment"/> (not <c>Environment.GetEnvironmentVariable</c>)
    ///     so the value is consistent with the ASP.NET Core hosting model.
    /// </param>
    public static IServiceCollection AddGatewayAuth(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        var authOptions = services.AddValidateOptions<JWTOptions>();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = !environment.IsDevelopment();
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SecretKey)),
                ValidIssuer = authOptions.Issuer,
                ValidAudience = authOptions.Audience,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("Authenticated",       policy => policy.RequireAuthenticatedUser())
            .AddPolicy(RoleConsts.Admin,      policy => policy.RequireRole(RoleConsts.Admin))
            .AddPolicy(RoleConsts.Owner,      policy => policy.RequireRole(RoleConsts.Owner))
            .AddPolicy(RoleConsts.User,       policy => policy.RequireRole(RoleConsts.User))
            .AddPolicy(RoleConsts.Moderator,  policy => policy.RequireRole(RoleConsts.Moderator, RoleConsts.Admin))
            .AddPolicy(RoleConsts.Employee,   policy => policy.RequireRole(RoleConsts.Employee))
            .AddPolicy(RoleConsts.Roaster,    policy => policy.RequireRole(RoleConsts.Roaster));

        return services;
    }
}
