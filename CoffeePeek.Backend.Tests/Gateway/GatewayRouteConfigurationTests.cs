using System.Text.Json;
using FluentAssertions;

namespace CoffeePeek.Backend.Tests.Gateway;

public class GatewayRouteConfigurationTests
{
    [Fact]
    public void GatewayRoutes_ShouldMatchExistingModerationControllers()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetGatewaySettingsPath()));
        var routes = document.RootElement
            .GetProperty("ReverseProxy")
            .GetProperty("Routes");

        var routePaths = routes.EnumerateObject()
            .Select(route => route.Value.GetProperty("Match").GetProperty("Path").GetString())
            .ToArray();

        routePaths.Should().Contain("/api/ModerationReviews/{**remainder}");
        routePaths.Should().Contain("/api/ModerationShops/{**remainder}");
        routePaths.Should().NotContain("/api/Moderation/{**remainder}");
    }

    [Theory]
    [InlineData("account-cluster")]
    [InlineData("shops-cluster")]
    [InlineData("moderation-cluster")]
    [InlineData("media-cluster")]
    public void GatewayClusters_ShouldUseProductionHealthPath(string clusterName)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(GetGatewaySettingsPath()));
        var cluster = document.RootElement
            .GetProperty("ReverseProxy")
            .GetProperty("Clusters")
            .GetProperty(clusterName);

        var healthPath = cluster
            .GetProperty("HealthCheck")
            .GetProperty("Active")
            .GetProperty("Path")
            .GetString();

        healthPath.Should().Be("/health");
    }

    private static string GetGatewaySettingsPath()
    {
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../CoffeePeek.Gateway/appsettings.json"));
    }
}
