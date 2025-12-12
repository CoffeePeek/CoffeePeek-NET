using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Tests.Integration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using City = CoffeePeek.ShopsService.Entities.City;

namespace CoffeePeek.E2ETests;

public class MapApiE2ETests(GatewayWebApplicationFactory gatewayFactory)
    : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly GatewayWebApplicationFactory _gatewayFactory = gatewayFactory;
    private readonly HttpClient _gatewayClient = gatewayFactory.CreateClient();
    private ShopsServiceWebApplicationFactory? _shopsServiceFactory;

    private async Task ClearDatabaseAsync()
    {
        if (_shopsServiceFactory == null) return;

        using var scope = _shopsServiceFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

        // Delete all data in reverse order of dependencies
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
    }

    [Fact]
    public async Task E2E_GetShopsInBounds_WithValidBounds_ReturnsShopsInBounds_ThroughGateway()
    {
        // Arrange - Start ShopsService
        _shopsServiceFactory = new ShopsServiceWebApplicationFactory();
        await _shopsServiceFactory.InitializeAsync();

        try
        {
            var shopsClient = _shopsServiceFactory.CreateClient();
            var shopsBaseAddress = shopsClient.BaseAddress?.ToString() ?? "http://localhost:5003";
            var shopsUri = new Uri(shopsBaseAddress);

            // Configure Gateway to route to ShopsService
            Environment.SetEnvironmentVariable("SHOPS_HOST", shopsUri.Host);
            Environment.SetEnvironmentVariable("SHOPS_PORT", shopsUri.Port.ToString());

            await ClearDatabaseAsync();

            using var scope = _shopsServiceFactory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

            // Create test city first
            var city = new City
            {
                Id = Guid.NewGuid(),
                Name = "Test City"
            };
            dbContext.Cities.Add(city);

            // Create test shops with coordinates
            var shop1 = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Shop 1",
                CityId = city.Id
            };
            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                ShopId = shop1.Id,
                Address = "Moscow, Red Square",
                Latitude = 55.7558m,
                Longitude = 37.6173m
            };
            shop1.LocationId = location1.Id;

            var shop2 = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Shop 2",
                CityId = city.Id
            };
            var location2 = new Location
            {
                Id = Guid.NewGuid(),
                ShopId = shop2.Id,
                Address = "Moscow, Kremlin",
                Latitude = 55.7520m,
                Longitude = 37.6156m
            };
            shop2.LocationId = location2.Id;

            // Shop outside bounds
            var shop3 = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Shop 3",
                CityId = city.Id
            };
            var location3 = new Location
            {
                Id = Guid.NewGuid(),
                ShopId = shop3.Id,
                Address = "St. Petersburg",
                Latitude = 59.9343m,
                Longitude = 30.3351m
            };
            shop3.LocationId = location3.Id;

            dbContext.Shops.AddRange(shop1, shop2, shop3);
            dbContext.Locations.AddRange(location1, location2, location3);
            await dbContext.SaveChangesAsync();

            // Act - Request shops in Moscow area through Gateway
            // Gateway routes /api/shops/map to /api/CoffeeShop/map in ShopsService
            var response = await _gatewayClient.GetAsync(
                $"/api/shops/map?minLat=55.7&minLon=37.6&maxLat=55.8&maxLon=37.7");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Shops.Should().HaveCount(2);
            result.Data.Shops.Should().Contain(s => s.Id == shop1.Id);
            result.Data.Shops.Should().Contain(s => s.Id == shop2.Id);
            result.Data.Shops.Should().NotContain(s => s.Id == shop3.Id);
        }
        finally
        {
            if (_shopsServiceFactory != null)
            {
                await _shopsServiceFactory.DisposeAsync();
            }
        }
    }

    [Fact]
    public async Task E2E_GetShopsInBounds_WithNoShopsInBounds_ReturnsEmpty_ThroughGateway()
    {
        // Arrange
        _shopsServiceFactory = new ShopsServiceWebApplicationFactory();
        await _shopsServiceFactory.InitializeAsync();

        try
        {
            var shopsClient = _shopsServiceFactory.CreateClient();
            var shopsBaseAddress = shopsClient.BaseAddress?.ToString() ?? "http://localhost:5003";
            var shopsUri = new Uri(shopsBaseAddress);

            Environment.SetEnvironmentVariable("SHOPS_HOST", shopsUri.Host);
            Environment.SetEnvironmentVariable("SHOPS_PORT", shopsUri.Port.ToString());

            await ClearDatabaseAsync();

            using var scope = _shopsServiceFactory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

            var city = new City
            {
                Id = Guid.NewGuid(),
                Name = "Test City"
            };
            dbContext.Cities.Add(city);

            // Create shop outside the requested bounds
            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Shop Outside",
                CityId = city.Id
            };
            var location = new Location
            {
                Id = Guid.NewGuid(),
                ShopId = shop.Id,
                Address = "Far Away",
                Latitude = 59.9343m,
                Longitude = 30.3351m
            };
            shop.LocationId = location.Id;

            dbContext.Shops.Add(shop);
            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            // Act - Request shops in different area through Gateway
            var response = await _gatewayClient.GetAsync(
                $"/api/shops/map?minLat=55.7&minLon=37.6&maxLat=55.8&maxLon=37.7");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Shops.Should().BeEmpty();
        }
        finally
        {
            if (_shopsServiceFactory != null)
            {
                await _shopsServiceFactory.DisposeAsync();
            }
        }
    }

    [Fact]
    public async Task E2E_GetShopsInBounds_Respects500Limit_ThroughGateway()
    {
        // Arrange
        _shopsServiceFactory = new ShopsServiceWebApplicationFactory();
        await _shopsServiceFactory.InitializeAsync();

        try
        {
            var shopsClient = _shopsServiceFactory.CreateClient();
            var shopsBaseAddress = shopsClient.BaseAddress?.ToString() ?? "http://localhost:5003";
            var shopsUri = new Uri(shopsBaseAddress);

            Environment.SetEnvironmentVariable("SHOPS_HOST", shopsUri.Host);
            Environment.SetEnvironmentVariable("SHOPS_PORT", shopsUri.Port.ToString());

            await ClearDatabaseAsync();

            using var scope = _shopsServiceFactory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

            var city = new City
            {
                Id = Guid.NewGuid(),
                Name = "Test City"
            };
            dbContext.Cities.Add(city);

            // Create 600 shops in bounds
            var shops = new List<Shop>();
            var locations = new List<Location>();

            for (int i = 0; i < 600; i++)
            {
                var shop = new Shop
                {
                    Id = Guid.NewGuid(),
                    Name = $"Shop {i}",
                    CityId = city.Id
                };
                var location = new Location
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    Address = $"Address {i}",
                    Latitude = 55.7558m + (i * 0.0001m), // Spread them slightly
                    Longitude = 37.6173m + (i * 0.0001m)
                };
                shop.LocationId = location.Id;
                shops.Add(shop);
                locations.Add(location);
            }

            dbContext.Shops.AddRange(shops);
            dbContext.Locations.AddRange(locations);
            await dbContext.SaveChangesAsync();

            // Act - Request through Gateway
            var response = await _gatewayClient.GetAsync(
                $"/api/shops/map?minLat=55.7&minLon=37.6&maxLat=55.9&maxLon=37.8");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Shops.Should().HaveCount(500); // Should be limited to 500
        }
        finally
        {
            if (_shopsServiceFactory != null)
            {
                await _shopsServiceFactory.DisposeAsync();
            }
        }
    }
}
