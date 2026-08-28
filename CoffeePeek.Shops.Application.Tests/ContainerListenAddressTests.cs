using System;
using System.IO;
using FluentAssertions;

namespace CoffeePeek.Shops.Application.Tests;

public class ContainerListenAddressTests
{
    [Fact]
    public void ShopsDockerfile_BindsAllInterfacesOnPort80()
    {
        var dockerfile = File.ReadAllText(Path.Combine(FindRepoRoot(), "CoffeePeek.ShopsService", "ShopsService.Dockerfile"));

        dockerfile.Should().Contain("ASPNETCORE_URLS=http://+:80");
        dockerfile.Should().NotContain("http://[::]:80");
        dockerfile.Should().Contain("ASPNETCORE_HTTP_PORTS=80");
    }

    [Fact]
    public void ProductionCompose_ForcesPort80AndExplicitShopsAddress()
    {
        var compose = File.ReadAllText(Path.Combine(FindRepoRoot(), "deploy", "docker-compose.yml"));

        compose.Should().Contain("ASPNETCORE_URLS: http://+:80");
        compose.Should().Contain("ASPNETCORE_HTTP_PORTS: \"80\"");
        compose.Should().Contain("http://shops:80");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "CoffeePeek.slnx")))
                return dir.FullName;

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root (CoffeePeek.slnx).");
    }
}
