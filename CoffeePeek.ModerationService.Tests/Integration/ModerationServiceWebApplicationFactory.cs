using CoffeePeek.ModerationService.Configuration;
using CoffeePeek.ModerationService.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Testcontainers.PostgreSql;
using Xunit;

namespace CoffeePeek.ModerationService.Tests.Integration;

public class ModerationServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("moderation_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly Mock<IYandexGeocodingService> _yandexGeocodingServiceMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();

    public Mock<IYandexGeocodingService> YandexGeocodingServiceMock => _yandexGeocodingServiceMock;
    public Mock<IPublishEndpoint> PublishEndpointMock => _publishEndpointMock;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the real YandexGeocodingService
            services.RemoveAll<IYandexGeocodingService>();
            
            // Add the mocked service
            services.AddSingleton(_yandexGeocodingServiceMock.Object);

            // Remove MassTransit services and add mocked IPublishEndpoint
            services.RemoveAll(typeof(IPublishEndpoint));
            services.RemoveAll(typeof(IBus));
            services.RemoveAll(typeof(IBusControl));
            services.AddSingleton(_publishEndpointMock.Object);

            // Replace database with test container
            services.RemoveAll(typeof(DbContextOptions<ModerationDbContext>));
            services.AddDbContext<ModerationDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Override authentication to allow test requests
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "Test", options => { });

            // Set Test as the default authentication scheme
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            });
        });

        builder.UseEnvironment("Testing");
    }


    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        // Create database schema from model (no migrations needed for tests)
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ModerationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}

// Test authentication handler that always succeeds
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Use a fixed test user ID for consistency across requests
        var userId = "00000000-0000-0000-0000-000000000001";
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Role, "Admin") // Add Admin role for tests that need it
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}