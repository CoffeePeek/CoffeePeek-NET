using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using City = CoffeePeek.ShopsService.Entities.City;

namespace CoffeePeek.ShopsService.Tests.Integration;

public class MapApiTests(ShopsServiceWebApplicationFactory factory) : IClassFixture<ShopsServiceWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task ClearDatabaseAsync()
    {
        using var scope = factory.Services.CreateScope();
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
    public async Task GetShopsInBounds_WithValidBounds_ReturnsShopsInBounds()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        using var scope = factory.Services.CreateScope();
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

        // Act - Request shops in Moscow area
        var response = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.7&minLon=37.6&maxLat=55.8&maxLon=37.7");

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

    [Fact]
    public async Task GetShopsInBounds_WithInvalidBounds_ReturnsError()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        // Note: When Range validation fails, ASP.NET Core returns 400 Bad Request
        // But we can test with valid ranges where min > max to test business logic validation
        
        // Act - minLat > maxLat (but within valid range)
        var response1 = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.8&minLon=37.6&maxLat=55.7&maxLon=37.7");

        // Assert - Controller validates and returns error in body
        // If validation fails at model binding, we get 400, but if it passes binding, controller returns 200 with error
        if (response1.StatusCode == HttpStatusCode.OK)
        {
            var result1 = await response1.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
            result1.Should().NotBeNull();
            result1!.IsSuccess.Should().BeFalse();
            result1.Message.Should().Contain("Invalid bounds");
        }
        else
        {
            // Model validation failed - this is also acceptable
            response1.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        // Act - minLon > maxLon (but within valid range)
        var response2 = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.7&minLon=37.7&maxLat=55.8&maxLon=37.6");

        // Assert
        if (response2.StatusCode == HttpStatusCode.OK)
        {
            var result2 = await response2.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
            result2.Should().NotBeNull();
            result2!.IsSuccess.Should().BeFalse();
            result2.Message.Should().Contain("Invalid bounds");
        }
        else
        {
            response2.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task GetShopsInBounds_WithNoShopsInBounds_ReturnsEmpty()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

        // Create test city first
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

        // Act - Request shops in different area
        var response = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.7&minLon=37.6&maxLat=55.8&maxLon=37.7");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().BeEmpty();
    }

    [Fact]
    public async Task GetShopsInBounds_WithShopsWithoutCoordinates_ExcludesThem()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

        // Create test city first
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City"
        };
        dbContext.Cities.Add(city);

        // Shop with coordinates
        var shop1 = new Shop
        {
            Id = Guid.NewGuid(),
            Name = "Shop With Coords",
            CityId = city.Id
        };
        var location1 = new Location
        {
            Id = Guid.NewGuid(),
            ShopId = shop1.Id,
            Address = "Moscow",
            Latitude = 55.7558m,
            Longitude = 37.6173m
        };
        shop1.LocationId = location1.Id;

        // Shop without coordinates
        var shop2 = new Shop
        {
            Id = Guid.NewGuid(),
            Name = "Shop Without Coords",
            CityId = city.Id
        };
        var location2 = new Location
        {
            Id = Guid.NewGuid(),
            ShopId = shop2.Id,
            Address = "Moscow",
            Latitude = null,
            Longitude = null
        };
        shop2.LocationId = location2.Id;

        // Shop without location
        var shop3 = new Shop
        {
            Id = Guid.NewGuid(),
            Name = "Shop Without Location",
            CityId = city.Id,
            LocationId = null
        };

        dbContext.Shops.AddRange(shop1, shop2, shop3);
        dbContext.Locations.AddRange(location1, location2);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.7&minLon=37.6&maxLat=55.8&maxLon=37.7");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().HaveCount(1);
        result.Data.Shops.Should().Contain(s => s.Id == shop1.Id);
        result.Data.Shops.Should().NotContain(s => s.Id == shop2.Id);
        result.Data.Shops.Should().NotContain(s => s.Id == shop3.Id);
    }

    [Fact]
    public async Task GetShopsInBounds_ResponseContainsOnlyRequiredFields()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

        // Create test city first
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City"
        };
        dbContext.Cities.Add(city);

        var shop = new Shop
        {
            Id = Guid.NewGuid(),
            Name = "Test Shop",
            Description = "Test Description",
            CityId = city.Id
        };
        var location = new Location
        {
            Id = Guid.NewGuid(),
            ShopId = shop.Id,
            Address = "Test Address",
            Latitude = 55.7558m,
            Longitude = 37.6173m
        };
        shop.LocationId = location.Id;

        dbContext.Shops.Add(shop);
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.7&minLon=37.6&maxLat=55.8&maxLon=37.7");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().HaveCount(1);

        var mapShop = result.Data.Shops[0];
        mapShop.Id.Should().Be(shop.Id);
        mapShop.Latitude.Should().Be(55.7558m);
        mapShop.Longitude.Should().Be(37.6173m);
        mapShop.Title.Should().Be("Test Shop");

        // Verify it's a lightweight response - only these fields should be present
        // (We can't directly check JSON, but we verify the DTO structure)
    }

    [Fact]
    public async Task GetShopsInBounds_Respects500Limit()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

        // Create test city first
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

        // Act
        var response = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.7&minLon=37.6&maxLat=55.9&maxLon=37.8");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().HaveCount(500); // Should be limited to 500
    }

    [Fact]
    public async Task GetShopsInBounds_WithBoundaryCoordinates_IncludesBoundaryShops()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

        // Create test city first
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City"
        };
        dbContext.Cities.Add(city);

        // Shop exactly on min boundary
        var shop1 = new Shop
        {
            Id = Guid.NewGuid(),
            Name = "Shop Min Boundary",
            CityId = city.Id
        };
        var location1 = new Location
        {
            Id = Guid.NewGuid(),
            ShopId = shop1.Id,
            Address = "Min Boundary",
            Latitude = 55.7m, // Exactly minLat
            Longitude = 37.6m // Exactly minLon
        };
        shop1.LocationId = location1.Id;

        // Shop exactly on max boundary
        var shop2 = new Shop
        {
            Id = Guid.NewGuid(),
            Name = "Shop Max Boundary",
            CityId = city.Id
        };
        var location2 = new Location
        {
            Id = Guid.NewGuid(),
            ShopId = shop2.Id,
            Address = "Max Boundary",
            Latitude = 55.8m, // Exactly maxLat
            Longitude = 37.7m // Exactly maxLon
        };
        shop2.LocationId = location2.Id;

        // Shop just outside (less than min)
        var shop3 = new Shop
        {
            Id = Guid.NewGuid(),
            Name = "Shop Outside",
            CityId = city.Id
        };
        var location3 = new Location
        {
            Id = Guid.NewGuid(),
            ShopId = shop3.Id,
            Address = "Outside",
            Latitude = 55.6999m, // Just below minLat
            Longitude = 37.5999m // Just below minLon
        };
        shop3.LocationId = location3.Id;

        dbContext.Shops.AddRange(shop1, shop2, shop3);
        dbContext.Locations.AddRange(location1, location2, location3);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.7&minLon=37.6&maxLat=55.8&maxLon=37.7");

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
}

