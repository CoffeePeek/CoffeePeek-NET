using CoffeePeek.Shared.Kernel;
using Yarp.ReverseProxy.Configuration;

namespace CoffeePeek.Gateway;

public static class YarpClusterFactory
{
    private record ClusterInfo(string ClusterId, string ServiceName);

    private static readonly List<ClusterInfo> Clusters =
    [
        new("account-cluster", AppResources.AccountService),
        new("shops-cluster", AppResources.ShopsService),
        new("moderation-cluster", AppResources.ModerationService),
        new("media-cluster", AppResources.MediaService),
    ];

    public static ClusterConfig[] CreateClusters()
    {
        return Clusters.Select(cluster => new ClusterConfig
        {
            ClusterId = cluster.ClusterId,
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { 
                    "destination1", 
                    new DestinationConfig 
                    { 
                        Address = $"http://{cluster.ServiceName}" 
                    } 
                }
            }
        }).ToArray();
    }
}