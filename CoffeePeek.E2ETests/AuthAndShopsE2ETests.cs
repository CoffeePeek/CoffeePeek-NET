using CoffeePeek.AuthService.Configuration;
using CoffeePeek.AuthService.Commands;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Response.Login;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Tests.Integration;
using FluentAssertions;
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
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Xunit;
using City = CoffeePeek.ShopsService.Entities.City;

namespace CoffeePeek.E2ETests;

public class AuthAndShopsE2ETests(GatewayWebApplicationFactory gatewayFactory)
    : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly GatewayWebApplicationFactory _gatewayFactory = gatewayFactory;
    private readonly HttpClient _gatewayClient = gatewayFactory.CreateClient();

    [Fact]
    public async Task E2E_AuthFlow_RegisterLoginRefresh_ThroughGateway()
    {
        await using var authFactory = new AuthServiceTestFactory();
        await authFactory.InitializeAsync();

        var authClient = authFactory.CreateClient();
        var authBase = authClient.BaseAddress ?? new Uri("http://localhost");
        Environment.SetEnvironmentVariable("AUTH_HOST", authBase.Host);
        Environment.SetEnvironmentVariable("AUTH_PORT", authBase.Port.ToString());

        var email = $"user_{Guid.NewGuid():N}@example.com";
        var password = "StrongP@ssw0rd!";
        var userName = "E2E User";

        // Register through Gateway
        var registerResponse = await _gatewayClient.PostAsJsonAsync("/api/auth/register",
            new RegisterUserCommand(email, password, userName));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<CreateEntityResponse<Guid>>();
        registerResult.Should().NotBeNull();
        registerResult!.IsSuccess.Should().BeTrue();
        var userId = registerResult.EntityId;

        // Login through Gateway
        var loginResponse = await _gatewayClient.PostAsJsonAsync("/api/auth/login",
            new LoginUserCommand(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Contract.Responses.Response<LoginResponse>>();
        loginResult.Should().NotBeNull();
        loginResult!.IsSuccess.Should().BeTrue();
        var refreshToken = loginResult.Data!.RefreshToken;

        // Refresh through Gateway
        var refreshRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/auth/refresh?refreshToken={refreshToken}");
        refreshRequest.Headers.Add("X-Test-UserId", userId.ToString());

        var refreshResponse = await _gatewayClient.SendAsync(refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<Contract.Responses.Response<GetRefreshTokenResponse>>();
        refreshResult.Should().NotBeNull();
        refreshResult!.IsSuccess.Should().BeTrue();
        refreshResult.Data.Should().NotBeNull();
        refreshResult.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshResult.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task E2E_ShopsFlow_GetListAndById_ThroughGateway()
    {
        var shopsFactory = new ShopsServiceWebApplicationFactory();
        await shopsFactory.InitializeAsync();
        try
        {
            var shopsClient = shopsFactory.CreateClient();
            var shopsBase = shopsClient.BaseAddress ?? new Uri("http://localhost");
            Environment.SetEnvironmentVariable("SHOPS_HOST", shopsBase.Host);
            Environment.SetEnvironmentVariable("SHOPS_PORT", shopsBase.Port.ToString());

            await SeedShopAsync(shopsFactory);

            var listResponse = await _gatewayClient.GetAsync("/api/shops");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var listResult = await listResponse.Content.ReadFromJsonAsync<Contract.Responses.Response<GetCoffeeShopsResponse>>();
            listResult.Should().NotBeNull();
            listResult!.IsSuccess.Should().BeTrue();
            listResult.Data.Should().NotBeNull();
            var shopId = listResult.Data!.CoffeeShops.First().Id;

            var byIdResponse = await _gatewayClient.GetAsync($"/api/shops/{shopId}");
            byIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var byIdResult = await byIdResponse.Content.ReadFromJsonAsync<Contract.Responses.Response<GetCoffeeShopResponse>>();
            byIdResult.Should().NotBeNull();
            byIdResult!.IsSuccess.Should().BeTrue();
            byIdResult.Data.Should().NotBeNull();
            byIdResult.Data!.Shop.Id.Should().Be(shopId);
        }
        finally
        {
            await shopsFactory.DisposeAsync();
        }
    }

    private static async Task SeedShopAsync(ShopsServiceWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

        dbContext.CheckIns.RemoveRange(dbContext.CheckIns);
        dbContext.Reviews.RemoveRange(dbContext.Reviews);
        dbContext.ShopScheduleIntervals.RemoveRange(dbContext.ShopScheduleIntervals);
        dbContext.ShopSchedules.RemoveRange(dbContext.ShopSchedules);
        dbContext.ShopPhotos.RemoveRange(dbContext.ShopPhotos);
        dbContext.ShopEquipments.RemoveRange(dbContext.ShopEquipments);
        dbContext.ShopBrewMethods.RemoveRange(dbContext.ShopBrewMethods);
        dbContext.RoasterShops.RemoveRange(dbContext.RoasterShops);
        dbContext.CoffeeBeanShops.RemoveRange(dbContext.CoffeeBeanShops);
        dbContext.Locations.RemoveRange(dbContext.Locations);
        dbContext.ShopContacts.RemoveRange(dbContext.ShopContacts);
        dbContext.Shops.RemoveRange(dbContext.Shops);
        dbContext.Cities.RemoveRange(dbContext.Cities);
        await dbContext.SaveChangesAsync();

        var cityId = Guid.NewGuid();
        var shopId = Guid.NewGuid();

        var city = new City { Id = cityId, Name = "Gateway City" };
        var shop = new Shop { Id = shopId, Name = "Gateway Shop", CityId = cityId };
        var location = new Location
        {
            Id = Guid.NewGuid(),
            ShopId = shopId,
            Address = "Gateway street",
            Latitude = 55.75m,
            Longitude = 37.61m
        };
        shop.LocationId = location.Id;

        dbContext.Cities.Add(city);
        dbContext.Shops.Add(shop);
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();
    }
}

internal class AuthServiceTestFactory : WebApplicationFactory<CoffeePeek.AuthService.Services.GoogleAuthService>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AuthDbContext>));
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseInMemoryDatabase($"AuthServiceE2E-{Guid.NewGuid()}");
            });

            services.RemoveAll(typeof(IRedisService));
            services.AddSingleton<IRedisService, E2EInMemoryRedisService>();

            var descriptors = services.Where(s =>
                s.ServiceType.Namespace?.Contains("MassTransit") == true ||
                s.ImplementationType?.Namespace?.Contains("MassTransit") == true ||
                s.ImplementationInstance?.GetType().Namespace?.Contains("MassTransit") == true).ToList();
            foreach (var d in descriptors) services.Remove(d);

            services.AddMassTransit(x =>
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            });

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, E2ETestAuthHandler>("Test", _ => { });

            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            });
        });
    }

    public Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        db.Database.EnsureCreated();
        return Task.CompletedTask;
    }

    public new Task DisposeAsync() => Task.CompletedTask;
}

internal class E2EInMemoryRedisService : IRedisService
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
        foreach (var key in keys) _storage.Remove(key);
        return Task.CompletedTask;
    }
}

internal class E2ETestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public E2ETestAuthHandler(
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
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

