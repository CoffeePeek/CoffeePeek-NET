using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Auth.Extensions;
using CoffeePeek.Shared.Web.Extensions;
using CoffeePeek.Shared.Web.Handlers;

namespace CoffeePeek.ModerationService;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        
        // Controllers and API
        services.AddControllersModule();

        // User context for reading claims from headers (set by Gateway)
        services.AddHeaderUserContext();

        // Authorization policies (JWT validation happens in Gateway)
        services.AddAuthorizationBuilder()
            .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
            .AddPolicy(RoleConsts.Owner, policy => policy.RequireRole(RoleConsts.Owner))
            .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User))
            .AddPolicy(RoleConsts.Moderator, policy => policy.RequireRole(RoleConsts.Moderator))
            .AddPolicy(RoleConsts.Employee, policy => policy.RequireRole(RoleConsts.Employee))
            .AddPolicy(RoleConsts.Roaster, policy => policy.RequireRole(RoleConsts.Roaster));

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IWebHostBuilder ConfigureWebhost(this IWebHostBuilder builder)
    {
        builder.ConfigureEnvironment();
        builder.UseSentry();

        return builder;
    }
}
