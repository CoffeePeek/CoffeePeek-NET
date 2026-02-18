using System.Text;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Auth.Extensions;
using CoffeePeek.Shared.Auth.Options;
using CoffeePeek.Shared.Kernel.Extentions;
using CoffeePeek.Shared.Web.Extensions;
using CoffeePeek.Shared.Web.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace CoffeePeek.AccountService;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddOperationTransformer((operation, context, _) =>
            {
                var hasAuthorize =
                    context.Description.ActionDescriptor.EndpointMetadata
                        .OfType<AuthorizeAttribute>()
                        .Any();

                if (!hasAuthorize)
                    return Task.CompletedTask;

                operation.Security ??= new List<OpenApiSecurityRequirement>();

                var schemeReference = new OpenApiSecuritySchemeReference("Bearer");

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [schemeReference] = []
                });

                return Task.CompletedTask;
            });
            
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Servers.Clear();
                document.Servers.Add(new OpenApiServer
                {
                    Url = "/",
                    Description = "Gateway"
                });

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Please enter a valid token"
                };

                return Task.CompletedTask;
            });
        });
        
        // Controllers and API
        services.AddControllersModule();

        services.AddJwtAuth();
        // User context for reading claims from headers (set by Gateway)
        services.AddHeaderUserContext();

        // Authorization policies (JWT validation happens in Gateway)
        
        services.AddAuthorizationBuilder()
            .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
            .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));

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
    
    private static IServiceCollection AddJwtAuth(this IServiceCollection services)
    {
        var authOptions = services.AddValidateOptions<JWTOptions>();

        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = !isDevelopment;
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
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}