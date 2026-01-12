using CoffeePeek.Shared.Extensions.Configuration;
using Yarp.ReverseProxy.Configuration;

namespace CoffeePeek.Gateway;

public static class YarpClusterFactory
{
    private const string DefaultPort = "80";
    
    private static readonly bool IsAspire =
        string.Equals(Environment.GetEnvironmentVariable("DOTNET_ASPIRE_RUNNING"), "true", StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_ASPIRE_RESOURCE_NAME"))
        || string.Equals(Environment.GetEnvironmentVariable("DOTNET_ASPIRE"), "true", StringComparison.OrdinalIgnoreCase);
    
    private record ClusterInfo(
        string ClusterId,
        string EnvPrefix,
        string AspireServiceName
    );

    private static readonly List<ClusterInfo> Clusters =
    [
        new("account-cluster", "ACCOUNT", AppResources.AccountService),
        new("shops-cluster", "SHOPS", AppResources.ShopsService),
        new("moderation-cluster", "MODERATION", AppResources.ModerationService),
        new("jobs-cluster", "JOBS", AppResources.JobVacanciesService)
    ];


    public static ClusterConfig[] CreateClusters()
    {
        var clusters = new List<ClusterConfig>();
        foreach (var cluster in Clusters)
        {
            var host = ResolveHost(cluster);
            var port = ResolvePort(cluster);

            // Prefer service URI published by Aspire (services__{name}__http/__https)
            var serviceUri = TryGetServiceUri(cluster.AspireServiceName);

            var address = serviceUri ?? BuildAddress(host, port);
            
            clusters.Add(new ClusterConfig
            {
                ClusterId = cluster.ClusterId,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "destination1", new DestinationConfig { Address = address } }
                }
            });
        }
        return clusters.ToArray();
    }
    
    private static string BuildAddress(string host, string? port)
    {
        if (IsAspire)
            return $"http://{host}";

        return string.IsNullOrWhiteSpace(port) ? DefaultPort : $"http://{host}:{port}";
    }

    
    private static string? ResolvePort(ClusterInfo cluster)
    {
        if (IsAspire)
            return null;

        var port = Environment.GetEnvironmentVariable($"{cluster.EnvPrefix}_PORT");
        
        return !string.IsNullOrWhiteSpace(port) ? port : DefaultPort;
    }
    
    private static string ResolveHost(ClusterInfo cluster)
    {
        var envHost = Environment.GetEnvironmentVariable($"{cluster.EnvPrefix}_HOST");
        if (!string.IsNullOrEmpty(envHost))
            return envHost;

        if (IsAspire)
            return cluster.AspireServiceName;

        // Fallback to Railway internal DNS pattern
        var defaultRailwayHost = Environment.GetEnvironmentVariable($"{cluster.EnvPrefix}_HOST");
        if (!string.IsNullOrEmpty(defaultRailwayHost))
            return defaultRailwayHost;

        throw new InvalidOperationException(
            $"Host configuration not found for {cluster.ClusterId}. " +
            $"Please set either {cluster.EnvPrefix}_HOST or {cluster.EnvPrefix}_HOST environment variable.");
    }

    private static string? TryGetServiceUri(string serviceName)
    {
        var keys = new[]
        {
            $"services__{serviceName}__https__0",
            $"services__{serviceName}__http__0",
            $"services__{serviceName}__https",
            $"services__{serviceName}__http"
        };

        foreach (var key in keys)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.TrimEnd('/'); // YARP expects base address without trailing slash
            }
        }

        return null;
    }

}