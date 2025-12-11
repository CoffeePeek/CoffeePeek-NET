using Yarp.ReverseProxy.Configuration;

namespace CoffeePeek.Gateway;

public static class YarpRouteFactory
{
    private record ServiceRoute(string Id, string PathPrefix, string ClusterId, string? TargetPath = null);

    // Список всех микросервисов, которые нужно проксировать
    private static readonly List<ServiceRoute> Services =
    [
        new("auth", "auth", "auth-cluster"),
        new("user", "user", "user-cluster"),
        // Преобразование пути: /api/shops/ -> /api/CoffeeShop/
        new("shops", "shops", "shops-cluster", "/api/CoffeeShop/{**catch-all}"),
        new("checkin", "CheckIn", "shops-cluster"), // CheckIn также идет в shops-cluster
        new("moderation", "moderation", "moderation-cluster"),
        // Преобразование пути: /api/photo/ -> /api/
        new("photo", "photo", "photo-cluster", "/api/{**catch-all}"),
        new("jobs", "vacancies", "jobs-cluster", "/api/vacancies/{**catch-all}")
    ];

    public static RouteConfig[] CreateRoutes()
    {
        var routes = new List<RouteConfig>();

        foreach (var service in Services)
        {
            routes.Add(CreateApiRoute(service));

            routes.Add(CreateSwaggerRoute(service));
        }

        routes.Add(new RouteConfig
        {
            RouteId = "gateway-swagger-ui",
            Match = new RouteMatch
            {
                Path = "/swagger/{**catch-all}"
            },
            Order = 5 
        });
    
        routes.Add(new RouteConfig
        {
            RouteId = "gateway-swagger-json",
            // ClusterId = null, 
            Match = new RouteMatch
            {
                Path = "/swagger/v1/swagger.json"
            },
            Order = 4
        });

        routes.Add(new RouteConfig
        {
            RouteId = "web-route",
            ClusterId = "web-cluster",
            Match = new RouteMatch { Path = "/{**catch-all}" },
            Order = 1000
        });

        return routes.ToArray();
    }

    private static RouteConfig CreateApiRoute(ServiceRoute service)
    {
        var route = new RouteConfig
        {
            RouteId = $"{service.Id}-route",
            ClusterId = service.ClusterId,
            Match = new RouteMatch { Path = $"/api/{service.PathPrefix}/{{**catch-all}}" },
            Transforms = service.TargetPath == null ? null : new List<Dictionary<string, string>> { new() { { "PathPattern", service.TargetPath } } }
        };

        return route;
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
}