using CoffeePeek.Gateway;
using CoffeePeek.Gateway.Extensions;
using CoffeePeek.Shared.Extensions.Handlers;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Extensions.Logging;
using CoffePeek.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSerilogLogging();

builder.ConfigureEnvironment();

builder.Services.AddReverseProxy().LoadFromMemory(YarpConfig.GetRoutes(), YarpConfig.GetClusters());

builder.Services.AddResponseCaching();
builder.Services.AddSwaggerModule("CoffeePeek Gateway API");
builder.Services.AddCorsModule();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseExceptionHandler();

app.UseCors();

app.UseResponseCaching();
app.ConfigureCustomCaching();

    
// Gateway self health check
app.MapGet("/health/gateway", () => Results.Ok(new { status = "healthy", service = "Gateway", timestamp = DateTime.UtcNow }))
    .WithName("GatewayHealthCheck")
    .WithTags("Health");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API");

    foreach (var service in YarpRouteFactory.ServicesList)
    {
        options.SwaggerEndpoint(
            $"/swagger/{service.Id}/v1/swagger.json", 
            $"{service.Id.ToUpper()} Service"
        );
    }
});

app.MapReverseProxy();

app.Run();
