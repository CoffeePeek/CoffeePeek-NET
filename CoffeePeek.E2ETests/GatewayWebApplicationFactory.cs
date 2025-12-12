using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace CoffeePeek.E2ETests;

public class GatewayWebApplicationFactory : WebApplicationFactory<CoffeePeek.Gateway.Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _authPostgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("auth_e2e_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly PostgreSqlContainer _userPostgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("user_e2e_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly PostgreSqlContainer _shopsPostgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("shops_e2e_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly PostgreSqlContainer _moderationPostgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("moderation_e2e_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    // Note: For E2E tests, we'll use a simplified approach
    // Services will be started separately or mocked
    // This factory is for Gateway only

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Gateway doesn't need special configuration for E2E tests
            // It will proxy to the test services
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Start all PostgreSQL containers
        // These will be used by individual service factories in E2E tests
        await _authPostgresContainer.StartAsync();
        await _userPostgresContainer.StartAsync();
        await _shopsPostgresContainer.StartAsync();
        await _moderationPostgresContainer.StartAsync();

        // For now, Gateway will be configured to route to test services
        // Individual service factories will be created in test classes
        // This is a placeholder for future E2E test infrastructure
    }

    public new async Task DisposeAsync()
    {
        await _authPostgresContainer.DisposeAsync();
        await _userPostgresContainer.DisposeAsync();
        await _shopsPostgresContainer.DisposeAsync();
        await _moderationPostgresContainer.DisposeAsync();

        await base.DisposeAsync();
    }
}

