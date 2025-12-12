using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Handlers.CoffeeShop;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoffeePeek.ShopsService.Tests.Handlers;

public class GetShopsInBoundsHandlerTests : IDisposable
{
    private readonly ShopsDbContext _dbContext;
    private readonly GetShopsInBoundsHandler _sut;

    public GetShopsInBoundsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ShopsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ShopsDbContext(options);
        _sut = new GetShopsInBoundsHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WithShopsInBounds_ReturnsShops()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var shop1Id = Guid.NewGuid();
        var shop1 = new Shop
        {
            Id = shop1Id,
            Name = "Shop 1",
            CityId = cityId,
            Location = new Location
            {
                Id = Guid.NewGuid(),
                Address = "Address 1",
                Latitude = 55.7558m,
                Longitude = 37.6173m,
                ShopId = shop1Id
            }
        };
        shop1.LocationId = shop1.Location.Id;
        var shop2Id = Guid.NewGuid();
        var shop2 = new Shop
        {
            Id = shop2Id,
            Name = "Shop 2",
            CityId = cityId,
            Location = new Location
            {
                Id = Guid.NewGuid(),
                Address = "Address 2",
                Latitude = 55.7500m,
                Longitude = 37.6000m,
                ShopId = shop2Id
            }
        };
        shop2.LocationId = shop2.Location.Id;
        var shop3Id = Guid.NewGuid();
        var shop3 = new Shop
        {
            Id = shop3Id,
            Name = "Shop 3",
            CityId = cityId,
            Location = new Location
            {
                Id = Guid.NewGuid(),
                Address = "Address 3",
                Latitude = 60.0m, // Outside bounds
                Longitude = 40.0m, // Outside bounds
                ShopId = shop3Id
            }
        };
        shop3.LocationId = shop3.Location.Id;

        _dbContext.Shops.AddRange(shop1, shop2, shop3);
        await _dbContext.SaveChangesAsync();

        var request = new GetShopsInBoundsRequest(
            MinLat: 55.7m,
            MinLon: 37.5m,
            MaxLat: 55.8m,
            MaxLon: 37.7m
        );

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().HaveCount(2);
        result.Data.Shops.Should().Contain(s => s.Id == shop1.Id && s.Title == "Shop 1");
        result.Data.Shops.Should().Contain(s => s.Id == shop2.Id && s.Title == "Shop 2");
        result.Data.Shops.Should().NotContain(s => s.Id == shop3.Id);
    }

    [Fact]
    public async Task Handle_WithShopsWithoutLocation_ExcludesThem()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var shop1Id = Guid.NewGuid();
        var shop1 = new Shop
        {
            Id = shop1Id,
            Name = "Shop 1",
            CityId = cityId,
            Location = new Location
            {
                Id = Guid.NewGuid(),
                Address = "Address 1",
                Latitude = 55.7558m,
                Longitude = 37.6173m,
                ShopId = shop1Id
            }
        };
        shop1.LocationId = shop1.Location.Id;
        var shop2 = new Shop
        {
            Id = Guid.NewGuid(),
            Name = "Shop 2",
            CityId = cityId,
            Location = null
        };

        _dbContext.Shops.AddRange(shop1, shop2);
        await _dbContext.SaveChangesAsync();

        var request = new GetShopsInBoundsRequest(
            MinLat: 55.7m,
            MinLon: 37.5m,
            MaxLat: 55.8m,
            MaxLon: 37.7m
        );

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().HaveCount(1);
        result.Data.Shops.Should().Contain(s => s.Id == shop1.Id);
        result.Data.Shops.Should().NotContain(s => s.Id == shop2.Id);
    }

    [Fact]
    public async Task Handle_WithShopsWithoutCoordinates_ExcludesThem()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var shop1Id = Guid.NewGuid();
        var shop1 = new Shop
        {
            Id = shop1Id,
            Name = "Shop 1",
            CityId = cityId,
            Location = new Location
            {
                Id = Guid.NewGuid(),
                Address = "Address 1",
                Latitude = 55.7558m,
                Longitude = 37.6173m,
                ShopId = shop1Id
            }
        };
        shop1.LocationId = shop1.Location.Id;
        var shop2Id = Guid.NewGuid();
        var shop2 = new Shop
        {
            Id = shop2Id,
            Name = "Shop 2",
            CityId = cityId,
            Location = new Location
            {
                Id = Guid.NewGuid(),
                Address = "Address 2",
                Latitude = null,
                Longitude = null,
                ShopId = shop2Id
            }
        };
        shop2.LocationId = shop2.Location.Id;

        _dbContext.Shops.AddRange(shop1, shop2);
        await _dbContext.SaveChangesAsync();

        var request = new GetShopsInBoundsRequest(
            MinLat: 55.7m,
            MinLon: 37.5m,
            MaxLat: 55.8m,
            MaxLon: 37.7m
        );

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().HaveCount(1);
        result.Data.Shops.Should().Contain(s => s.Id == shop1.Id);
        result.Data.Shops.Should().NotContain(s => s.Id == shop2.Id);
    }

    [Fact]
    public async Task Handle_WithNoShopsInBounds_ReturnsEmptyArray()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var shop = new Shop
        {
            Id = shopId,
            Name = "Shop 1",
            CityId = cityId,
            Location = new Location
            {
                Id = Guid.NewGuid(),
                Address = "Address 1",
                Latitude = 60.0m, // Outside bounds
                Longitude = 40.0m, // Outside bounds
                ShopId = shopId
            }
        };
        shop.LocationId = shop.Location.Id;

        _dbContext.Shops.Add(shop);
        await _dbContext.SaveChangesAsync();

        var request = new GetShopsInBoundsRequest(
            MinLat: 55.7m,
            MinLon: 37.5m,
            MaxLat: 55.8m,
            MaxLon: 37.7m
        );

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsLightweightResponse()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var shop = new Shop
        {
            Id = shopId,
            Name = "Test Shop",
            CityId = cityId,
            Location = new Location
            {
                Id = Guid.NewGuid(),
                Address = "Test Address",
                Latitude = 55.7558m,
                Longitude = 37.6173m,
                ShopId = shopId
            }
        };
        shop.LocationId = shop.Location.Id;

        _dbContext.Shops.Add(shop);
        await _dbContext.SaveChangesAsync();

        var request = new GetShopsInBoundsRequest(
            MinLat: 55.7m,
            MinLon: 37.5m,
            MaxLat: 55.8m,
            MaxLon: 37.7m
        );

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().HaveCount(1);

        var shopDto = result.Data.Shops[0];
        shopDto.Id.Should().Be(shop.Id);
        shopDto.Latitude.Should().Be(55.7558m);
        shopDto.Longitude.Should().Be(37.6173m);
        shopDto.Title.Should().Be("Test Shop");
    }

    [Fact]
    public async Task Handle_WithManyShops_RespectsLimit()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var shops = new List<Shop>();
        for (int i = 0; i < 600; i++)
        {
            var shopId = Guid.NewGuid();
            var location = new Location
            {
                Id = Guid.NewGuid(),
                Address = $"Address {i}",
                Latitude = 55.7558m + (i * 0.0001m),
                Longitude = 37.6173m + (i * 0.0001m),
                ShopId = shopId
            };
            var shop = new Shop
            {
                Id = shopId,
                Name = $"Shop {i}",
                CityId = cityId,
                Location = location,
                LocationId = location.Id
            };
            shops.Add(shop);
        }

        _dbContext.Shops.AddRange(shops);
        await _dbContext.SaveChangesAsync();

        var request = new GetShopsInBoundsRequest(
            MinLat: 55.0m,
            MinLon: 37.0m,
            MaxLat: 56.0m,
            MaxLon: 38.0m
        );

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shops.Should().HaveCountLessThanOrEqualTo(500);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}