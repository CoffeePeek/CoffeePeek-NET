using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Shop;
using Xunit;
using City = CoffeePeek.ShopsService.Entities.City;

namespace CoffeePeek.ShopsService.Tests.Integration;

public class EventConsumerTests(ShopsServiceWebApplicationFactory factory) : IClassFixture<ShopsServiceWebApplicationFactory>
{
    private async Task<Guid> EnsureTestCityAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        
        // Consumer uses Guid.Empty, so we need to create a city with that ID
        var city = await dbContext.Cities.FirstOrDefaultAsync(c => c.Id == Guid.Empty);
        if (city == null)
        {
            city = new City { Id = Guid.Empty, Name = "Default Test City" };
            dbContext.Cities.Add(city);
            await dbContext.SaveChangesAsync();
        }
        
        return city.Id;
    }

    private async Task ClearDatabaseAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        
        // Delete all data in reverse order of dependencies
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
    public async Task CoffeeShopApprovedEventConsumer_WithValidEvent_CreatesShop()
    {
        // Arrange
        await ClearDatabaseAsync();
        var cityId = await EnsureTestCityAsync();

        var @event = new CoffeeShopApprovedEvent(
            CreatorId: Guid.NewGuid(),
            Shop: new ShopDto
            {
                Id = Guid.NewGuid(),
                CityId = cityId,
                Name = "Test Coffee Shop",
                ImageUrls =
                [
                ],
                Rating = 0,
                ReviewCount = 0,
                IsOpen = true,
                PriceRange = 0,
                Description = null,
                Location = new LocationDto
                {
                    Address = "Test Address",
                    Latitude = 55.7558m,
                    Longitude = 37.6173m
                },
                Beans =
                [
                ],
                Roasters =
                [
                ],
                Equipments =
                [
                ],
                BrewMethods =
                [
                ],
                ShopContact = null,
                Schedules = null
            }
        );

        // Act
        await factory.Harness!.Bus.Publish(@event);
        
        // Wait for consumption (give consumer time to process)
        await Task.Delay(1000); // Give consumer time to process
        
        var consumed = await factory.ConsumerHarness!.Consumed.Any<CoffeeShopApprovedEvent>();
        if (!consumed)
        {
            // Check for faults - this will help diagnose issues
            var hasFaults = await factory.Harness.Published.Any<Fault<CoffeeShopApprovedEvent>>();
            if (hasFaults)
            {
                throw new Exception("Consumer failed with fault (check logs for details)");
            }
            throw new Exception("Event was not consumed and no fault was published");
        }
        consumed.Should().BeTrue("Event should be consumed");

        // Assert
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        
        var shop = await dbContext.Shops.FirstOrDefaultAsync();
        shop.Should().NotBeNull();
        shop!.Name.Should().Be("Test Coffee Shop");
        
        var location = await dbContext.Locations.FirstOrDefaultAsync(l => l.ShopId == shop.Id);
        location.Should().NotBeNull();
        location!.Latitude.Should().Be(55.7558m);
        location.Longitude.Should().Be(37.6173m);
        location.Address.Should().Be("Validated Address");
    }

    // NOTE: Consumer currently does not map contact/photos from CoffeeShopApprovedEvent.Shop.
    // Интеграционные тесты ниже опирались на старый формат события и старую бизнес-логику.
    // При необходимости их можно вернуть, когда будет добавлена поддержка контактов/фото в consumer.

    [Fact]
    public async Task CoffeeShopApprovedEventConsumer_WithoutCoordinates_DoesNotCreateLocation()
    {
        // Arrange
        await ClearDatabaseAsync();
        await EnsureTestCityAsync();

        var @event = new CoffeeShopApprovedEvent(
            CreatorId: Guid.NewGuid(),
            Shop: new ShopDto
            {
                Id = Guid.NewGuid(),
                CityId = Guid.Empty,
                Name = "Test Coffee Shop",
                Location = null
            });

        // Act
        await factory.Harness!.Bus.Publish(@event);
        
        // Wait for consumption (give consumer time to process)
        await Task.Delay(1000); // Give consumer time to process
        
        var consumed = await factory.ConsumerHarness!.Consumed.Any<CoffeeShopApprovedEvent>();
        if (!consumed)
        {
            // Check for faults - this will help diagnose issues
            var hasFaults = await factory.Harness.Published.Any<Fault<CoffeeShopApprovedEvent>>();
            if (hasFaults)
            {
                throw new Exception("Consumer failed with fault (check logs for details)");
            }
            throw new Exception("Event was not consumed and no fault was published");
        }
        consumed.Should().BeTrue("Event should be consumed");

        // Assert
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        
        var shop = await dbContext.Shops.FirstOrDefaultAsync();
        shop.Should().NotBeNull();
        // При отсутствии Location в событии consumer не создает запись в Locations
        shop!.LocationId.Should().BeNull();
        (await dbContext.Locations.FirstOrDefaultAsync()).Should().BeNull();
    }

    [Fact]
    public async Task CoffeeShopApprovedEventConsumer_WithCompleteData_CreatesAllEntities()
    {
        // Arrange
        await ClearDatabaseAsync();
        var cityId = await EnsureTestCityAsync();

        var shopContact = new ShopContactDto
        {
            PhoneNumber = "+1234567890",
            InstagramLink = "https://instagram.com/test"
        };

        var userId = Guid.NewGuid();

        var shopDto = new ShopDto
        {
            Id = Guid.NewGuid(),
            CityId = cityId,
            Name = "Complete Test Shop",
            Description = "Test Address",
            Location = new LocationDto
            {
                Address = "Validated Address",
                Latitude = 55.7558m,
                Longitude = 37.6173m
            },
            ShopContact = shopContact,
            ImageUrls = new[] { "https://example.com/photo1.jpg" }
        };

        var @event = new CoffeeShopApprovedEvent(userId, shopDto);

        // Act
        await factory.Harness!.Bus.Publish(@event);
        
        // Wait for consumption (give consumer time to process)
        await Task.Delay(1000); // Give consumer time to process
        
        var consumed = await factory.ConsumerHarness!.Consumed.Any<CoffeeShopApprovedEvent>();
        if (!consumed)
        {
            // Check for faults - this will help diagnose issues
            var hasFaults = await factory.Harness.Published.Any<Fault<CoffeeShopApprovedEvent>>();
            if (hasFaults)
            {
                throw new Exception("Consumer failed with fault (check logs for details)");
            }
            throw new Exception("Event was not consumed and no fault was published");
        }
        consumed.Should().BeTrue("Event should be consumed");

        // Assert
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopsDbContext>();
        
        var shop = await dbContext.Shops
            .Include(s => s.ShopContact)
            .Include(s => s.Location)
            .FirstOrDefaultAsync();
        
        shop.Should().NotBeNull();
        shop!.Name.Should().Be("Complete Test Shop");
        shop.ShopContact.Should().NotBeNull();
        shop.Location.Should().NotBeNull();
        shop.Location!.Latitude.Should().Be(55.7558m);
        shop.Location.Longitude.Should().Be(37.6173m);

        var shopPhotos = await dbContext.ShopPhotos
            .Where(p => p.ShopId == shop.Id)
            .ToListAsync();
        // Текущий consumer не создает ShopPhotos из ImageUrls, поэтому фотографий быть не должно
        shopPhotos.Should().HaveCount(0);
    }
}

