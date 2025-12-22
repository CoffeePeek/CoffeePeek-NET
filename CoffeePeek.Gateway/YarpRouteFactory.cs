using System.Collections.ObjectModel;
using Yarp.ReverseProxy.Configuration;

namespace CoffeePeek.Gateway;

public static class YarpRouteFactory
{
    public record ServiceRoute(string Id, string[] Controllers, string ClusterId);
    
    private static readonly List<ServiceRoute> Services =
    [
        new("account", ["Auth", "User"], "account-cluster"),
        new("shops", ["CheckIn", "CoffeeShop", "Internal", "Review"], "shops-cluster"),
        new("moderation", ["Moderation"], "moderation-cluster"),
        new("jobs", ["Vacancies"], "jobs-cluster")
    ];

    public static ReadOnlyCollection<ServiceRoute> Servicess => Services.AsReadOnly();

    public static RouteConfig[] CreateRoutes()
    {
        var routes = new List<RouteConfig>();

        foreach (var service in Services)
        {
            routes.AddRange(service.Controllers.Select(controller => CreateApiRoute(service, controller)));

            routes.Add(CreateSwaggerRoute(service));
        }
    
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
}