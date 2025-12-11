using Yarp.ReverseProxy.Configuration;

namespace CoffeePeek.Gateway;

public static class YarpClusterFactory
{
    private record ClusterInfo(string ClusterId, string ServiceHostVar, string DefaultHost);

    private static readonly List<ClusterInfo> Clusters =
    [
        new("auth-cluster", "AUTH_HOST", "coffeepeekauthservice.railway.internal"),
        new("user-cluster", "USER_HOST", "coffeepeekuserservice.railway.internal"),
        new("shops-cluster", "SHOPS_HOST", "coffeepeekshopsservice.railway.internal"),
        new("moderation-cluster", "MODERATION_HOST", "coffeepeekmoderationservice.railway.internal"),
        new("photo-cluster", "PHOTO_API_HOST", "coffeepeek-photo-api.railway.internal"),
        new("jobs-cluster", "JOBS_HOST", "coffeepeekjobvacancies.railway.internal"),
        new("web-cluster", "WEB_HOST", "coffeepeekweb.railway.internal")
    ];

    private const string DefaultPort = "80";
    private const string PortRailVarSuffix = "_PORT";

    public static ClusterConfig[] CreateClusters()
    {
        var clusters = new List<ClusterConfig>();
        foreach (var cluster in Clusters)
        {
            var host = GetServiceVariable(cluster.ServiceHostVar, cluster.DefaultHost);
            var port = GetServiceVariable(cluster.ServiceHostVar.Replace("_HOST", PortRailVarSuffix), DefaultPort);
            
            clusters.Add(new ClusterConfig
            {
                ClusterId = cluster.ClusterId,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "destination1", new DestinationConfig { Address = $"http://{host}:{port}" } }
                }
            });
        }
        return clusters.ToArray();
    }
    
    private static string GetServiceVariable(string hostVar, string defaultName)
    {
        return Environment.GetEnvironmentVariable(hostVar) 
            ?? Environment.GetEnvironmentVariable(hostVar.Replace("_SERVICE_", "_")) // Поиск по сокращенному имени (AUTH_HOST)
            ?? defaultName;
    }
}