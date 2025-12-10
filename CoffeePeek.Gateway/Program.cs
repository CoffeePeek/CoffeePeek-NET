using CoffeePeek.Shared.Extensions.Middleware;
using CoffeePeek.Shared.Extensions.Modules;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Environment configuration
builder.ConfigureEnvironment();

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromMemory(GetRoutes(), GetClusters());

// Swagger
builder.Services.AddSwaggerModule("CoffeePeek Gateway API", "v1");

// CORS
builder.Services.AddCorsModule();

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandling();

if (CorsModule.IsCorsEnabled())
{
    app.UseCors();
}

// Swagger documentation
var isDebug = Environment.GetEnvironmentVariable("IS_DEBUG");
var showSwagger = app.Environment.IsDevelopment() || 
                  (!string.IsNullOrEmpty(isDebug) && 
                   (isDebug.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                    isDebug.Equals("1", StringComparison.OrdinalIgnoreCase)));

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Swagger enabled: {ShowSwagger}, IsDevelopment: {IsDev}, IS_DEBUG: {IsDebug}", 
    showSwagger, app.Environment.IsDevelopment(), isDebug ?? "not set");

if (showSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API");
        c.SwaggerEndpoint("/swagger/auth/v1/swagger.json", "Auth Service");
        c.SwaggerEndpoint("/swagger/user/v1/swagger.json", "User Service");
        c.SwaggerEndpoint("/swagger/shops/v1/swagger.json", "Shops Service");
        c.SwaggerEndpoint("/swagger/moderation/v1/swagger.json", "Moderation Service");
        c.SwaggerEndpoint("/swagger/photo/v1/swagger.json", "Photo Service");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });
}

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Gateway", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("Health");

// Map reverse proxy routes
app.MapReverseProxy();

app.Run();
return;

// YARP Configuration
static RouteConfig[] GetRoutes()
{
    return
    [
        new RouteConfig
        {
            RouteId = "auth-route",
            ClusterId = "auth-cluster",
            Match = new RouteMatch
            {
                Path = "/api/auth/{**catch-all}"
            }
        },
        new RouteConfig
        {
            RouteId = "user-route",
            ClusterId = "user-cluster",
            Match = new RouteMatch
            {
                Path = "/api/user/{**catch-all}"
            }
        },
        new RouteConfig
        {
            RouteId = "shops-route",
            ClusterId = "shops-cluster",
            Match = new RouteMatch
            {
                Path = "/api/shops/{**catch-all}"
            },
            Transforms = new List<Dictionary<string, string>>
            {
                new()
                {
                    { "PathPattern", "/api/CoffeeShop/{**catch-all}" }
                }
            }
        },
        new RouteConfig
        {
            RouteId = "checkin-route",
            ClusterId = "shops-cluster",
            Match = new RouteMatch
            {
                Path = "/api/CheckIn/{**catch-all}"
            }
        },
        new RouteConfig
        {
            RouteId = "moderation-route",
            ClusterId = "moderation-cluster",
            Match = new RouteMatch
            {
                Path = "/api/moderation/{**catch-all}"
            }
        },
        new RouteConfig
        {
            RouteId = "photo-route",
            ClusterId = "photo-cluster",
            Match = new RouteMatch
            {
                Path = "/api/photo/{**catch-all}"
            },
            Transforms = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/api/{**catch-all}" }
                }
            }
        },
        // Swagger proxy routes for each service (must be before web-route)
        new RouteConfig
        {
            RouteId = "auth-swagger-route",
            ClusterId = "auth-cluster",
            Match = new RouteMatch
            {
                Path = "/swagger/auth/{**catch-all}"
            },
            Transforms = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/swagger/{**catch-all}" }
                }
            }
        },
        new RouteConfig
        {
            RouteId = "user-swagger-route",
            ClusterId = "user-cluster",
            Match = new RouteMatch
            {
                Path = "/swagger/user/{**catch-all}"
            },
            Transforms = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/swagger/{**catch-all}" }
                }
            }
        },
        new RouteConfig
        {
            RouteId = "shops-swagger-route",
            ClusterId = "shops-cluster",
            Match = new RouteMatch
            {
                Path = "/swagger/shops/{**catch-all}"
            },
            Transforms = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/swagger/{**catch-all}" }
                }
            }
        },
        new RouteConfig
        {
            RouteId = "moderation-swagger-route",
            ClusterId = "moderation-cluster",
            Match = new RouteMatch
            {
                Path = "/swagger/moderation/{**catch-all}"
            },
            Transforms = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/swagger/{**catch-all}" }
                }
            }
        },
        new RouteConfig
        {
            RouteId = "photo-swagger-route",
            ClusterId = "photo-cluster",
            Match = new RouteMatch
            {
                Path = "/swagger/photo/{**catch-all}"
            },
            Transforms = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/swagger/{**catch-all}" }
                }
            }
        },
        // Web route - exclude swagger and health endpoints
        new RouteConfig
        {
            RouteId = "web-route",
            ClusterId = "web-cluster",
            Match = new RouteMatch
            {
                Path = "/{**catch-all}"
            },
            Order = 1000
        }
    ];
}

static ClusterConfig[] GetClusters()
{
    var authHost = GetServiceHost("AUTH_SERVICE_HOST", "AUTH_HOST", "coffeepeekauthservice.railway.internal");
    var authPort = GetServicePort("AUTH_SERVICE_PORT", "AUTH_PORT", "80");
    
    var userHost = GetServiceHost("USER_SERVICE_HOST", "USER_HOST", "coffeepeekuserservice.railway.internal");
    var userPort = GetServicePort("USER_SERVICE_PORT", "USER_PORT", "80");
    
    var shopsHost = GetServiceHost("SHOPS_SERVICE_HOST", "SHOPS_HOST", "coffeepeekshopsservice.railway.internal");
    var shopsPort = GetServicePort("SHOPS_SERVICE_PORT", "SHOPS_PORT", "80");
    
    var moderationHost = GetServiceHost("MODERATION_SERVICE_HOST", "MODERATION_HOST", "coffeepeekmoderationservice.railway.internal");
    var moderationPort = GetServicePort("MODERATION_SERVICE_PORT", "MODERATION_PORT", "80");
    
    var photoHost = GetServiceHost("PHOTO_API_HOST", "PHOTO_HOST", "coffeepeek-photo-api.railway.internal");
    var photoPort = GetServicePort("PHOTO_API_PORT", "PHOTO_PORT", "80");
    
    var webHost = GetServiceHost("WEB_HOST", "WEB_HOST", "coffeepeekweb.railway.internal");
    var webPort = GetServicePort("WEB_PORT", "WEB_PORT", "80");

    return new[]
    {
        new ClusterConfig
        {
            ClusterId = "auth-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "destination1", new DestinationConfig { Address = $"http://{authHost}:{authPort}" } }
            }
        },
        new ClusterConfig
        {
            ClusterId = "user-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "destination1", new DestinationConfig { Address = $"http://{userHost}:{userPort}" } }
            }
        },
        new ClusterConfig
        {
            ClusterId = "shops-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "destination1", new DestinationConfig { Address = $"http://{shopsHost}:{shopsPort}" } }
            }
        },
        new ClusterConfig
        {
            ClusterId = "moderation-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "destination1", new DestinationConfig { Address = $"http://{moderationHost}:{moderationPort}" } }
            }
        },
        new ClusterConfig
        {
            ClusterId = "photo-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "destination1", new DestinationConfig { Address = $"http://{photoHost}:{photoPort}" } }
            }
        },
        new ClusterConfig
        {
            ClusterId = "web-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "destination1", new DestinationConfig { Address = $"http://{webHost}:{webPort}" } }
            }
        }
    };
}

static string GetServiceHost(string railwayVar, string customVar, string defaultName)
{
    return Environment.GetEnvironmentVariable(railwayVar) 
        ?? Environment.GetEnvironmentVariable(customVar) 
        ?? defaultName;
}

static string GetServicePort(string railwayVar, string customVar, string defaultPort)
{
    return Environment.GetEnvironmentVariable(railwayVar) 
        ?? Environment.GetEnvironmentVariable(customVar) 
        ?? defaultPort;
}
