using CoffeePeek.Gateway;
using CoffeePeek.Gateway.Extensions;
using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureEnvironment();

builder.Services.AddReverseProxy().LoadFromMemory(YarpConfig.GetRoutes(), YarpConfig.GetClusters());

builder.Services.AddResponseCaching();
builder.Services.AddSwaggerModule("CoffeePeek Gateway API", "v1");
builder.Services.AddCorsModule();

var app = builder.Build();

app.UseExceptionHandling();

if (CorsModule.IsCorsEnabled())
{
    app.UseCors();
}

app.UseResponseCaching();
app.ConfigureCustomCaching();

    
app.ConfigureSwaggerEndpoints(app.Services.GetRequiredService<ILogger<Program>>());

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Gateway", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.MapReverseProxy();

app.Run();