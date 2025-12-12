using CoffeePeek.AuthService.Configuration;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
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
using Xunit;
using Testcontainers.PostgreSql;

namespace CoffeePeek.AuthService.Tests.Integration;

public class AuthServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("auth_tests")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Replace database with test container
            services.RemoveAll(typeof(DbContextOptions<AuthDbContext>));
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Replace Redis with in-memory stub
            services.RemoveAll(typeof(IRedisService));
            services.AddSingleton<IRedisService, InMemoryRedisService>();

            // Remove MassTransit configuration and replace with in-memory transport
            var toRemove = services.Where(s =>
                s.ServiceType.Namespace?.Contains("MassTransit") == true ||
                s.ImplementationType?.Namespace?.Contains("MassTransit") == true ||
                s.ImplementationInstance?.GetType().Namespace?.Contains("MassTransit") == true).ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            services.AddMassTransit(x =>
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            });

            // Test authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });

            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        // Ensure database is created
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}

internal class InMemoryRedisService : IRedisService
{
    private readonly Dictionary<string, object> _storage = new();

    public Task<T?> GetAsync<T>(Shared.Infrastructure.Cache.CacheKey cacheKey)
    {
        return Task.FromResult(_storage.TryGetValue(cacheKey.ToString(), out var value) ? (T?)value : default);
    }

    public Task SetAsync<T>(Shared.Infrastructure.Cache.CacheKey cacheKey, T value, TimeSpan? customTtl = null)
    {
        _storage[cacheKey.ToString()] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Shared.Infrastructure.Cache.CacheKey cacheKey)
    {
        _storage.Remove(cacheKey.ToString());
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Shared.Infrastructure.Cache.CacheKey cacheKey)
    {
        return Task.FromResult(_storage.ContainsKey(cacheKey.ToString()));
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        var keys = _storage.Keys.Where(k => k.Contains(pattern)).ToList();
        foreach (var key in keys)
        {
            _storage.Remove(key);
        }
        return Task.CompletedTask;
    }
}

internal class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
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
        var userIdHeader = Request.Headers.TryGetValue("X-Test-UserId", out var header) && Guid.TryParse(header, out var parsed)
            ? parsed
            : Guid.Parse("00000000-0000-0000-0000-000000000001");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userIdHeader.ToString()),
            new Claim(ClaimTypes.Email, "test@example.com")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

