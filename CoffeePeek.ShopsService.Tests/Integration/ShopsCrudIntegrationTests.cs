using CoffeePeek.Contract.Constants;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Tests.Integration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using City = CoffeePeek.ShopsService.Entities.City;

namespace CoffeePeek.ShopsService.Tests.Integration;

public class ShopsCrudIntegrationTests(ShopsServiceWebApplicationFactory factory)
    : IClassFixture<ShopsServiceWebApplicationFactory>, IAsyncLifetime
{
    private readonly ShopsServiceWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();
    private Guid _cityId;
    private Guid _shopId;

    public async Task InitializeAsync()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        _client.DefaultRequestHeaders.Add("X-Test-UserId", Guid.Parse("00000000-0000-0000-0000-000000000001").ToString());
        await ClearDatabaseAsync();
        await SeedShopAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task ClearDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        var redisService = scope.ServiceProvider.GetRequiredService<CoffeePeek.Shared.Infrastructure.Interfaces.Redis.IRedisService>();

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

        // Clear Redis cache to prevent stale data
        await redisService.RemoveByPatternAsync(CacheKey.Shop.ByCityPattern(BusinessConstants.DefaultUnAuthorizedCityId));
    }

    private async Task SeedShopAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();

        // Use DefaultUnAuthorizedCityId so the shop is returned when querying without cityId parameter
        _cityId = BusinessConstants.DefaultUnAuthorizedCityId;
        _shopId = Guid.NewGuid();

        // Ensure the default city exists
        var city = await dbContext.Cities.FirstOrDefaultAsync(c => c.Id == _cityId);
        if (city == null)
        {
            city = new City
            {
                Id = _cityId,
                Name = "Default Test City"
            };
            dbContext.Cities.Add(city);
        }

        var shop = new Shop
        {
            Id = _shopId,
            Name = "Test Shop",
            CityId = _cityId
        };

        var location = new Location
        {
            Id = Guid.NewGuid(),
            ShopId = _shopId,
            Address = "Main street",
            Latitude = 55.75m,
            Longitude = 37.61m
        };
        shop.LocationId = location.Id;

        if (!dbContext.Cities.Any(c => c.Id == _cityId))
        {
            dbContext.Cities.Add(city);
        }
        dbContext.Shops.Add(shop);
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetCoffeeShops_ReturnsSeededShop()
    {
        var response = await _client.GetAsync("/api/CoffeeShop");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<GetCoffeeShopsResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CoffeeShops.Should().Contain(s => s.Id == _shopId && s.Name == "Test Shop");
    }

    [Fact]
    public async Task GetCoffeeShop_ById_ReturnsShop()
    {
        var response = await _client.GetAsync($"/api/CoffeeShop/{_shopId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<GetCoffeeShopResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shop.Id.Should().Be(_shopId);
    }

    [Fact]
    public async Task Review_CreateAndGet_Flow_Works()
    {
        // Create review
        var addRequest = new AddCoffeeShopReviewRequest
        {
            ShopId = _shopId,
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Header = "Great",
            Comment = "Tasty coffee",
            RatingCoffee = 5,
            RatingPlace = 4,
            RatingService = 5
        };

        var addResponse = await _client.PostAsJsonAsync("/api/ReviewCoffee", addRequest);
        if (addResponse.StatusCode != HttpStatusCode.OK)
        {
            var errorBody = await addResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Unexpected status {addResponse.StatusCode}: {errorBody}");
        }
        var addResult = await addResponse.Content.ReadFromJsonAsync<Response<AddCoffeeShopReviewResponse>>();
        addResult.Should().NotBeNull();
        addResult!.IsSuccess.Should().BeTrue();
        var reviewId = addResult.Data!.ReviewId;

        // Get by id
        var getResponse = await _client.GetAsync($"/api/ReviewCoffee/{reviewId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResult = await getResponse.Content.ReadFromJsonAsync<Response<GetReviewByIdResponse>>();
        getResult.Should().NotBeNull();
        getResult!.IsSuccess.Should().BeTrue();
        getResult.Data!.Review.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CheckIn_Create_ReturnsSuccess()
    {
        var request = new CreateCheckInRequest
        {
            ShopId = _shopId,
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Note = "Visiting"
        };

        var response = await _client.PostAsJsonAsync("/api/CheckIn", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<CreateCheckInResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CheckInId.Should().NotBeEmpty();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        (await dbContext.CheckIns.CountAsync()).Should().Be(1);
    }
}

