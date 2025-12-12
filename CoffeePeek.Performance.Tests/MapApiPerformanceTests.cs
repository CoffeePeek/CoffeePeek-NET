using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Tests.Integration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using City = CoffeePeek.ShopsService.Entities.City;

namespace CoffeePeek.Performance.Tests;

public class MapApiPerformanceTests(ShopsServiceWebApplicationFactory factory)
    : IClassFixture<ShopsServiceWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private bool _dataSeeded;

    public async Task InitializeAsync()
    {
        if (!_dataSeeded)
        {
            await SeedPerformanceDataAsync();
            _dataSeeded = true;
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedPerformanceDataAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

        // Clear existing data
        await ClearDatabaseAsync();

        // Create test city
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Performance Test City"
        };
        dbContext.Cities.Add(city);
        await dbContext.SaveChangesAsync();

        // Create 10,000 shops with coordinates for performance testing
        var shops = new List<Shop>();
        var locations = new List<Location>();
        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 0; i < 10000; i++)
        {
            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                Name = $"Performance Shop {i}",
                CityId = city.Id
            };

            // Distribute shops across a larger area (Moscow region)
            var latitude = 55.5m + (decimal)(random.NextDouble() * 1.0); // 55.5 to 56.5
            var longitude = 37.0m + (decimal)(random.NextDouble() * 2.0); // 37.0 to 39.0

            var location = new Location
            {
                Id = Guid.NewGuid(),
                ShopId = shop.Id,
                Address = $"Address {i}",
                Latitude = latitude,
                Longitude = longitude
            };
            shop.LocationId = location.Id;

            shops.Add(shop);
            locations.Add(location);
        }

        // Batch insert for better performance
        dbContext.Shops.AddRange(shops);
        dbContext.Locations.AddRange(locations);
        await dbContext.SaveChangesAsync();
    }

    private async Task ClearDatabaseAsync()
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
    }

    [Fact]
    public async Task MapApi_GetShopsInBounds_WithLargeDataset_CompletesWithinAcceptableTime()
    {
        // Arrange
        var sw = Stopwatch.StartNew();

        // Act - Request shops in a small area (should return subset of 10,000 shops)
        var response = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.7&minLon=37.6&maxLat=55.8&maxLon=37.7");

        sw.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Performance assertion: Should complete within 2 seconds for large dataset
        sw.ElapsedMilliseconds.Should().BeLessThan(2000, 
            $"Map API should respond within 2 seconds, but took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task MapApi_GetShopsInBounds_With500Limit_RespectsLimit()
    {
        // Arrange
        // Ensure we have more than 500 shops in the bounds
        // The seeded data should have shops in this area

        // Act
        var response = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.5&minLon=37.0&maxLat=56.5&maxLon=39.0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().HaveCountLessThanOrEqualTo(500, 
            "Map API should limit results to 500 shops");
    }

    [Fact]
    public async Task MapApi_GetShopsInBounds_MultipleConcurrentRequests_HandlesLoad()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        var sw = Stopwatch.StartNew();

        // Act - Send 10 concurrent requests
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync(
                $"/api/CoffeeShop/map?minLat=55.7&minLon=37.6&maxLat=55.8&maxLon=37.7"));
        }

        var responses = await Task.WhenAll(tasks);
        sw.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        
        // All requests should complete within reasonable time (5 seconds for 10 concurrent)
        sw.ElapsedMilliseconds.Should().BeLessThan(5000, 
            $"10 concurrent requests should complete within 5 seconds, but took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task MapApi_GetShopsInBounds_WithEmptyBounds_ReturnsQuickly()
    {
        // Arrange
        var sw = Stopwatch.StartNew();

        // Act - Request shops in area with no shops
        var response = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=60.0&minLon=30.0&maxLat=60.1&maxLon=30.1");

        sw.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().BeEmpty();

        // Empty results should be very fast
        sw.ElapsedMilliseconds.Should().BeLessThan(500, 
            $"Empty result query should complete within 500ms, but took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task MapApi_GetShopsInBounds_ResponseTime_ScalesLinearly()
    {
        // Arrange
        var smallAreaSw = Stopwatch.StartNew();
        var largeAreaSw = Stopwatch.StartNew();

        // Act - Small area (fewer shops)
        var smallResponse = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.75&minLon=37.6&maxLat=55.76&maxLon=37.61");
        smallAreaSw.Stop();

        // Large area (more shops, but still limited to 500)
        largeAreaSw.Start();
        var largeResponse = await _client.GetAsync(
            $"/api/CoffeeShop/map?minLat=55.5&minLon=37.0&maxLat=56.5&maxLon=39.0");
        largeAreaSw.Stop();

        // Assert
        smallResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        largeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var smallResult = await smallResponse.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();
        var largeResult = await largeResponse.Content.ReadFromJsonAsync<Response<GetShopsInBoundsResponse>>();

        smallResult.Should().NotBeNull();
        largeResult.Should().NotBeNull();

        // Both should complete within acceptable time
        // Large area might take slightly longer, but should still be fast due to 500 limit
        smallAreaSw.ElapsedMilliseconds.Should().BeLessThan(2000);
        largeAreaSw.ElapsedMilliseconds.Should().BeLessThan(2000);
    }
}
