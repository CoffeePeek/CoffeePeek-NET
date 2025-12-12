using CoffeePeek.ShopsService.Consumers;
using CoffeePeek.ShopsService.DB;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Testcontainers.PostgreSql;
using Xunit;

namespace CoffeePeek.ShopsService.Tests.Integration;

public class ShopsServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("shops_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public ITestHarness? Harness { get; private set; }
    public IConsumerTestHarness<CoffeeShopApprovedEventConsumer>? ConsumerHarness { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace database with test container
            services.RemoveAll(typeof(DbContextOptions<ShopsDbContext>));
            services.RemoveAll(typeof(CoffeePeek.Data.Interfaces.IUnitOfWork));
            services.AddDbContext<ShopsDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });
            // Ensure IUnitOfWork is registered (needed by consumer)
            services.AddScoped<CoffeePeek.Data.Interfaces.IUnitOfWork, CoffeePeek.Data.Repositories.UnitOfWork<ShopsDbContext>>();

            // Remove existing MassTransit configuration completely
            // Remove all MassTransit-related service descriptors
            var descriptorsToRemove = services.Where(s =>
            {
                var type = s.ServiceType;
                var implType = s.ImplementationType;
                var implInstance = s.ImplementationInstance;
                var implFactory = s.ImplementationFactory;
                
                return (type.Namespace?.Contains("MassTransit") == true) ||
                       (implType?.Namespace?.Contains("MassTransit") == true) ||
                       (implInstance?.GetType().Namespace?.Contains("MassTransit") == true);
            }).ToList();
            
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Configure MassTransit TestHarness for testing
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<CoffeeShopApprovedEventConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });

            // Override authentication to allow test requests (same approach as ModerationService)
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "Test", options => { });

            // Set Test as the default authentication scheme
            services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
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
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Start MassTransit test harness
        Harness = Services.GetRequiredService<ITestHarness>();
        ConsumerHarness = Services.GetRequiredService<IConsumerTestHarness<CoffeeShopApprovedEventConsumer>>();
        await Harness.Start();
    }

    public new async Task DisposeAsync()
    {
        if (Harness != null)
        {
            await Harness.Stop();
        }

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
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}