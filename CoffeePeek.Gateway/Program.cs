using CoffeePeek.Shared.Extensions.Middleware;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

#region PORT

// Configure PORT from environment variable
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port) && int.TryParse(port, out var portNumber))
{
    builder.WebHost.UseUrls($"http://*:{portNumber}");
}

#endregion

#region ALLOWED_HOSTS

// Configure AllowedHosts from environment variable
var allowedHosts = Environment.GetEnvironmentVariable("ALLOWED_HOSTS");
if (!string.IsNullOrEmpty(allowedHosts))
{
    builder.Configuration["AllowedHosts"] = allowedHosts;
}

#endregion

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromMemory(GetRoutes(), GetClusters());

// Configure CORS from environment variable
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
if (!string.IsNullOrEmpty(allowedOrigins))
{
    var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(origins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandling();

if (!string.IsNullOrEmpty(allowedOrigins))
{
    app.UseCors();
}

app.MapReverseProxy();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Gateway", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.Run();

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
                new Dictionary<string, string>
                {
                    { "PathPattern", "/api/CoffeeShop/{**catch-all}" }
                }
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
        new RouteConfig
        {
            RouteId = "web-route",
            ClusterId = "web-cluster",
            Match = new RouteMatch
            {
                Path = "/{**catch-all}"
            }
        }
    ];
}

static ClusterConfig[] GetClusters()
{
    // Get service hosts and ports from environment variables
    // In Railway, services are accessible via service discovery
    // Try Railway service variables first, then fallback to custom env vars, then service names
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

// Helper methods for service discovery
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
