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
        string RailwayHost,
        string AspireServiceName
    );

    private static readonly List<ClusterInfo> Clusters =
    [
        new("auth-cluster", "AUTH", "coffeepeekauthservice.railway.internal", AppResources.AuthService),
        new("user-cluster", "USER", "coffeepeekuserservice.railway.internal", AppResources.UserService),
        new("shops-cluster", "SHOPS", "coffeepeekshopsservice.railway.internal", AppResources.ShopsService),
        new("moderation-cluster", "MODERATION", "coffeepeekmoderationservice.railway.internal", AppResources.ModerationService),
        new("jobs-cluster", "JOBS", "coffeepeekjobvacancies.railway.internal", AppResources.JobVacanciesService)
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

        if (string.IsNullOrWhiteSpace(port))
            throw new InvalidOperationException($"Port is missing for host {host}");

        return $"http://{host}:{port}";
    }

    
    private static string? ResolvePort(ClusterInfo cluster)
    {
        if (IsAspire)
            return null;

        var port = Environment.GetEnvironmentVariable($"{cluster.EnvPrefix}_PORT");
        if (!string.IsNullOrWhiteSpace(port))
            return port;

        throw new InvalidOperationException(
            $"{cluster.EnvPrefix}_PORT is not set. Provide a port when running outside Aspire.");
    }
    
    private static string ResolveHost(ClusterInfo cluster)
    {
        var envHost = Environment.GetEnvironmentVariable($"{cluster.EnvPrefix}_HOST");
        if (!string.IsNullOrEmpty(envHost))
            return envHost;

        if (IsAspire)
            return cluster.AspireServiceName;

        // Local/dev fallback when not running under Aspire: assume localhost and require an explicit port.
        return "localhost";
    }

    private static string? TryGetServiceUri(string serviceName)
    {
        // Aspire publishes service endpoints into configuration/environment:
        // services__{serviceName}__https__0, services__{serviceName}__http__0, etc.
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