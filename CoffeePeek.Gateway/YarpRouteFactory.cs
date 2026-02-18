using System.Collections.ObjectModel;
using CoffeePeek.Shared.Web.Constants;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

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
        new("media", ["Photos"], "media-cluster", DefaultVersions)
    ];

    public static ReadOnlyCollection<ServiceRoute> ServicesList => Services.AsReadOnly();

    public static RouteConfig[] CreateRoutes()
    {
        var routes = new List<RouteConfig>();

        foreach (var service in Services)
        {
            routes.Add(CreateOpenApiRoute(service));

            foreach (var controller in service.Controllers)
            {
                routes.Add(new RouteConfig
                {
                    RouteId = $"{service.Id}-{controller}-route",
                    ClusterId = service.ClusterId,
                    Match = new RouteMatch
                    {
                        Path = $"/api/{controller}/{{**remainder}}"
                    }
                });
            }
        }

        return routes.ToArray();
    }

    public static RouteConfig CreateOpenApiRoute(ServiceRoute service)
    {
        return new RouteConfig
            {
                RouteId = $"{service.Id}-openapi-route",
                ClusterId = service.ClusterId,
                Match = new RouteMatch
                {
                    Path = $"/{service.Id}/openapi/{{**catch-all}}"
                }
            }
            .WithTransformPathRemovePrefix(prefix: $"/{service.Id}");
    }
}