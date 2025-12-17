using CoffeePeek.Gateway;
using CoffeePeek.Gateway.Extensions;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Logging;
using CoffePeek.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSerilogLogging();

builder.ConfigureEnvironment();

builder.Services.AddReverseProxy().LoadFromMemory(YarpConfig.GetRoutes(), YarpConfig.GetClusters());

builder.Services.AddResponseCaching();
builder.Services.AddSwaggerModule("CoffeePeek Gateway API", "v1");
builder.Services.AddCorsModule();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseExceptionHandling();

if (CorsModule.IsCorsEnabled())
{
    app.UseCors();
}

app.UseResponseCaching();
app.ConfigureCustomCaching();

    
app.ConfigureSwaggerEndpoints(app.Services.GetRequiredService<ILogger<Program>>());

// Gateway self health check
app.MapGet("/health/gateway", () => Results.Ok(new { status = "healthy", service = "Gateway", timestamp = DateTime.UtcNow }))
    .WithName("GatewayHealthCheck")
    .WithTags("Health");

app.MapReverseProxy();

app.Run();
