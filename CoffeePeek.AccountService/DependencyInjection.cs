using CoffeePeek.Shared.Extensions.Handlers;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Infrastructure.Constants;

namespace CoffeePeek.AccountService;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // Controllers and API
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // User context for reading claims from headers (set by Gateway)
        services.AddHeaderUserContext();

        // Authorization policies (JWT validation happens in Gateway)
        services.AddAuthorizationBuilder()
            .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
            .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));

        // Swagger
        services.AddSwaggerModule("CoffeePeek Account Service");

        // HTTP Context
        services.AddHttpContextAccessor();

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