using Yarp.ReverseProxy.Configuration;

namespace CoffeePeek.Gateway;

public static class YarpConfig
{
    public static RouteConfig[] GetRoutes() => YarpRouteFactory.CreateRoutes();
    public static ClusterConfig[] GetClusters() => YarpClusterFactory.CreateClusters();
}