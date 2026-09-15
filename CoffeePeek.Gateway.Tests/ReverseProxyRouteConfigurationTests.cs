using System.Text.Json;
using FluentAssertions;

namespace CoffeePeek.Gateway.Tests;

public class ReverseProxyRouteConfigurationTests
{
    [Fact]
    public void AppReleaseAutomationRoute_IsLimitedToReleaseListAndCreate()
    {
        using var document = LoadGatewayAppsettings();
        var routes = document.RootElement.GetProperty("ReverseProxy").GetProperty("Routes");

        routes.TryGetProperty("shops-admin-v1-app-downloads-automation-route", out var route)
            .Should().BeTrue();
        route.GetProperty("ClusterId").GetString().Should().Be("shops-cluster");
        route.GetProperty("AuthorizationPolicy").GetString().Should().Be("AppReleaseAutomationPolicy");
        route.GetProperty("Match").GetProperty("Path").GetString()
            .Should().Be("/api/admin/v1/app-downloads/android/releases");
        route.GetProperty("Match").GetProperty("Methods").EnumerateArray()
            .Select(method => method.GetString())
            .Should().Equal("GET", "POST");
    }

    [Theory]
    [InlineData("shops-admin-cities-route", "/api/admin/cities/{**remainder}")]
    [InlineData("shops-admin-equipments-route", "/api/admin/equipments/{**remainder}")]
    [InlineData("shops-admin-beans-route", "/api/admin/beans/{**remainder}")]
    [InlineData("shops-admin-roasters-route", "/api/admin/roasters/{**remainder}")]
    [InlineData("shops-admin-brew-methods-route", "/api/admin/brew-methods/{**remainder}")]
    [InlineData("shops-admin-coffee-zones-route", "/api/admin/coffee-zones/{**remainder}")]
    public void AdminCatalogCrudRoutes_TargetShopsService(string routeId, string path)
    {
        using var document = LoadGatewayAppsettings();
        var routes = document.RootElement.GetProperty("ReverseProxy").GetProperty("Routes");

        routes.TryGetProperty(routeId, out var route).Should().BeTrue();
        route.GetProperty("ClusterId").GetString().Should().Be("shops-cluster");
        route.GetProperty("AuthorizationPolicy").GetString().Should().Be("Moderator");
        route.GetProperty("Match").GetProperty("Path").GetString().Should().Be(path);
    }

    [Fact]
    public void AccountAdminCatchAll_RemainsAfterShopsAdminCatalogRoutes()
    {
        using var document = LoadGatewayAppsettings();
        var routeIds = document.RootElement
            .GetProperty("ReverseProxy")
            .GetProperty("Routes")
            .EnumerateObject()
            .Select(route => route.Name)
            .ToList();

        var accountAdminIndex = routeIds.IndexOf("account-admin-route");
        accountAdminIndex.Should().BeGreaterThan(-1);

        foreach (var shopsCatalogRoute in new[]
        {
            "shops-admin-cities-route",
            "shops-admin-equipments-route",
            "shops-admin-beans-route",
            "shops-admin-roasters-route",
            "shops-admin-brew-methods-route",
            "shops-admin-coffee-zones-route"
        })
        {
            routeIds.IndexOf(shopsCatalogRoute).Should().BeLessThan(accountAdminIndex);
        }
    }

    [Fact]
    public void ModerationRoastersRoute_TargetsModerationClusterWithSubmissionRateLimit()
    {
        using var document = LoadGatewayAppsettings();
        var routes = document.RootElement.GetProperty("ReverseProxy").GetProperty("Routes");

        routes.TryGetProperty("moderation-ModerationRoasters-route", out var route).Should().BeTrue();
        route.GetProperty("ClusterId").GetString().Should().Be("moderation-cluster");
        route.GetProperty("RateLimiterPolicy").GetString().Should().Be("moderation-submission");
        route.GetProperty("Match").GetProperty("Path").GetString().Should().Be("/api/ModerationRoasters/{**remainder}");
    }

    [Fact]
    public void PublicRoastersRoute_TargetsShopsCluster()
    {
        using var document = LoadGatewayAppsettings();
        var routes = document.RootElement.GetProperty("ReverseProxy").GetProperty("Routes");

        routes.TryGetProperty("shops-Roasters-route", out var route).Should().BeTrue();
        route.GetProperty("ClusterId").GetString().Should().Be("shops-cluster");
        route.GetProperty("Match").GetProperty("Path").GetString().Should().Be("/api/roasters/{**remainder}");
    }

    private static JsonDocument LoadGatewayAppsettings()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "CoffeePeek.Gateway",
            "appsettings.json"));

        return JsonDocument.Parse(File.ReadAllText(path));
    }
}
