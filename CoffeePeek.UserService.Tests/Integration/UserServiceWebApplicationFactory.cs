using CoffeePeek.UserService.Configuration;
using CoffeePeek.UserService.EventConsumer;
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
using System.Security.Claims;
using System.Text.Encodings.Web;
using Testcontainers.PostgreSql;
using Xunit;

namespace CoffeePeek.UserService.Tests.Integration;

public class UserServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("user_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public IBusControl? TestBus { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace database with test container
            services.RemoveAll(typeof(DbContextOptions<UserDbContext>));
            services.AddDbContext<UserDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Remove existing MassTransit configuration completely
            var descriptorsToRemove = services.Where(s =>
            {
                var type = s.ServiceType;
                var implType = s.ImplementationType;
                var implInstance = s.ImplementationInstance;
                
                return (type.Namespace?.Contains("MassTransit") == true) ||
                       (implType?.Namespace?.Contains("MassTransit") == true) ||
                       (implInstance?.GetType().Namespace?.Contains("MassTransit") == true);
            }).ToList();
            
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Configure MassTransit with InMemory transport for testing
            services.AddMassTransit(x =>
            {
                x.AddConsumer<UserRegisteredEventConsumer>();
                x.AddConsumer<CheckinCreatedEventConsumer>();
                x.AddConsumer<ReviewAddedEventConsumer>();
                x.AddConsumer<CoffeeShopApprovedEventConsumer>();
                
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });

            // Override authentication to allow test requests
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
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Get and start the bus
        TestBus = Services.GetRequiredService<IBusControl>();
        await TestBus.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        if (TestBus != null)
        {
            await TestBus.StopAsync();
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

