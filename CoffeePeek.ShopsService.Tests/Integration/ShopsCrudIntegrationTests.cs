using CoffeePeek.Contract.Constants;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Cache;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoffeePeek.Contract.Responses.CoffeeShop.Review;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Infrastructure.Configuration;
using CoffeePeek.Tests.Shared;
using Xunit;
using City = CoffeePeek.Shops.Domain.Entities.City;

namespace CoffeePeek.ShopsService.Tests.Integration;

public class ShopsCrudIntegrationTests(ShopsServiceWebApplicationFactory factory)
    : IClassFixture<ShopsServiceWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private Guid _cityId;
    private Guid _shopId;

    public async Task InitializeAsync()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        _client.DefaultRequestHeaders.Add("X-Test-UserId", Consts.UserTestGuidId.ToString());
        await ClearDatabaseAsync();
        await SeedShopAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task ClearDatabaseAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();

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
        await redisService.RemoveByPatternAsync(CacheKey.CachedShop.ByCityPattern(BusinessConstants.DefaultUnAuthorizedCityId));
    }

    private async Task SeedShopAsync()
    {
        using var scope = factory.Services.CreateScope();
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
            UserId = Consts.UserTestGuidId,
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
        // Id мапится из Guid в int через GetHashCode, поэтому может быть отрицательным и/или 0.
        // Достаточно проверить, что это не значение по умолчанию и что отзыв вообще загружен.
        getResult.Data!.Review.Id.Should().NotBe(0);
    }

    [Fact]
    public async Task CheckIn_Create_ReturnsSuccess()
    {
        var request = new CreateCheckInRequest
        {
            ShopId = _shopId,
            UserId = Consts.UserTestGuidId,
            Note = "Visiting"
        };

        var response = await _client.PostAsJsonAsync("/api/CheckIn", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Response<CreateCheckInResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CheckInId.Should().NotBeEmpty();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        (await dbContext.CheckIns.CountAsync()).Should().Be(1);
    }
}

