using System.Collections.ObjectModel;
using CoffeePeek.Shared.Infrastructure.Constants;
using Yarp.ReverseProxy.Configuration;

namespace CoffeePeek.Gateway;

public static class YarpRouteFactory
{
    public record ServiceRoute(string Id, string[] Controllers, string ClusterId, string[] Versions);

    private static string[] AllVersions => [ApiVersions.V1_0, ApiVersions.V2_0];
    private static string[] DefaultVersions => [ApiVersions.V1_0];

    private static readonly List<ServiceRoute> Services =
    [
        new("account", ["Tokens", "Users"], "account-cluster", DefaultVersions),
        new("shops",
            [
                "Catalogs",
                "CheckIns",
                "CoffeeShops",
                "CoffeeShopReviews",
                "FavoriteCoffeeShops",
                "Map",
                "UserReviews"
            ],
            "shops-cluster", DefaultVersions),
        new("moderation", ["Moderation", "ModerationReviews", "ModerationShops"], "moderation-cluster", DefaultVersions),
        new("media", ["Photo"], "media-cluster", DefaultVersions),
        new("jobs", ["Vacancies"], "jobs-cluster", DefaultVersions)
    ];

    public static ReadOnlyCollection<ServiceRoute> ServicesList => Services.AsReadOnly();

    public static RouteConfig[] CreateRoutes()
    {
        var routes = new List<RouteConfig>();

        foreach (var service in Services)
        {
            routes.AddRange(service.Controllers.Select(controller => CreateApiRoute(service, controller)));

            routes.Add(CreateSwaggerRoute(service));
        }

        // Admin routes with service-specific prefixes
        routes.Add(CreateAdminRoute("account", "account-cluster"));
        routes.Add(CreateAdminRoute("shops", "shops-cluster"));
        routes.Add(CreateAdminRoute("vacancies", "jobs-cluster"));
        routes.Add(CreateAdminRoute("moderation", "moderation-cluster"));

        return routes.ToArray();
    }

    private static RouteConfig CreateApiRoute(ServiceRoute service, string controller)
    {
        return new RouteConfig
        {
            RouteId = $"{service.Id}-{controller}-api-route",
            ClusterId = service.ClusterId,
            Match = new RouteMatch { Path = $"/api/{controller}/{{**catch-all}}" },
        };
    }

    private static RouteConfig CreateSwaggerRoute(ServiceRoute service) => new()
    {
        RouteId = $"{service.Id}-swagger-route",
        ClusterId = service.ClusterId,
        Match = new RouteMatch { Path = $"/swagger/{service.Id}/{{**catch-all}}" },
        Transforms = new List<Dictionary<string, string>>
        {
            new() { { "PathPattern", "/swagger/{**catch-all}" } }
        }
    };

    private static RouteConfig CreateAdminRoute(string serviceName, string clusterId) => new()
    {
        RouteId = $"admin-{serviceName}-route",
        ClusterId = clusterId,
        Match = new RouteMatch { Path = $"/api/admin/{serviceName}/{{**catch-all}}" }
    };
}