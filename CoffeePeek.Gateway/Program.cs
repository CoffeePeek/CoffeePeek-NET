using CoffeePeek.Gateway;
using CoffeePeek.Gateway.Extensions;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Web.Extensions;
using CoffeePeek.Shared.Web.Handlers;
using CoffeePeek.Shared.Web.Logging;
using CoffePeek.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSerilogLogging();

builder.ConfigureEnvironment();

// JWT Authentication for Gateway
builder.Services.AddGatewayJwtAuth();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin))
    .AddPolicy(RoleConsts.Owner, policy => policy.RequireRole(RoleConsts.Owner))
    .AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User))
    .AddPolicy(RoleConsts.Moderator, policy => policy.RequireRole(RoleConsts.Moderator))
    .AddPolicy(RoleConsts.Employee, policy => policy.RequireRole(RoleConsts.Employee))
    .AddPolicy(RoleConsts.Roaster, policy => policy.RequireRole(RoleConsts.Roaster));

builder.Services.AddReverseProxy()
    .LoadFromMemory(YarpConfig.GetRoutes(), YarpConfig.GetClusters())
    .AddTransforms<ClaimsToHeadersTransformProvider>();

builder.Services.AddResponseCaching();
builder.Services.AddCorsModule();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseExceptionHandler();

app.UseCors();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCaching();


// Gateway self health check
app.MapGet("/health/gateway", () => Results.Ok(new { status = "healthy", service = "Gateway", timestamp = DateTime.UtcNow }))
    .WithName("GatewayHealthCheck")
    .WithTags("Health");

app.MapReverseProxy();

app.Run();
