using CoffeePeek.Gateway;
using CoffeePeek.Gateway.Extensions;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

builder.ConfigureEnvironment();

builder.Services.AddReverseProxy().LoadFromMemory(YarpConfig.GetRoutes(), YarpConfig.GetClusters());

builder.Services.AddResponseCaching();
builder.Services.AddSwaggerModule("CoffeePeek Gateway API", "v1");
builder.Services.AddCorsModule();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "self" });

var app = builder.Build();

app.UseExceptionHandling();

if (CorsModule.IsCorsEnabled())
{
    app.UseCors();
}

app.UseResponseCaching();
app.ConfigureCustomCaching();

    
app.ConfigureSwaggerEndpoints(app.Services.GetRequiredService<ILogger<Program>>());

// Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("self")
});

// Gateway self health check
app.MapGet("/health/gateway", () => Results.Ok(new { status = "healthy", service = "Gateway", timestamp = DateTime.UtcNow }))
    .WithName("GatewayHealthCheck")
    .WithTags("Health");

app.MapReverseProxy();

app.Run();
