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
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Coffee Shop",
            NotValidatedAddress: "Test Address",
            UserId: Guid.NewGuid(),
            address: "Validated Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: new List<string>(),
            Schedules: new List<CoffeePeek.Contract.Dtos.Schedule.ScheduleDto>(),
            Latitude: 55.7558m,
            Longitude: 37.6173m
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

    [Fact]
    public async Task CoffeeShopApprovedEventConsumer_WithShopContact_CreatesContact()
    {
        // Arrange
        await ClearDatabaseAsync();
        await EnsureTestCityAsync();

        var shopContact = new ShopContactDto
        {
            PhoneNumber = "+1234567890",
            InstagramLink = "https://instagram.com/test"
        };

        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Coffee Shop",
            NotValidatedAddress: "Test Address",
            UserId: Guid.NewGuid(),
            address: "Validated Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: shopContact,
            ShopPhotos: new List<string>(),
            Schedules: new List<CoffeePeek.Contract.Dtos.Schedule.ScheduleDto>(),
            Latitude: 55.7558m,
            Longitude: 37.6173m
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
        
        var shop = await dbContext.Shops
            .Include(s => s.ShopContact)
            .FirstOrDefaultAsync();
        shop.Should().NotBeNull();
        shop!.ShopContact.Should().NotBeNull();
        shop.ShopContact!.PhoneNumber.Should().Be("+1234567890");
        shop.ShopContact.InstagramLink.Should().Be("https://instagram.com/test");
    }

    [Fact]
    public async Task CoffeeShopApprovedEventConsumer_WithShopPhotos_CreatesPhotos()
    {
        // Arrange
        await ClearDatabaseAsync();
        await EnsureTestCityAsync();

        var photos = new List<string> { "https://example.com/photo1.jpg", "https://example.com/photo2.jpg" };
        var userId = Guid.NewGuid();

        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Coffee Shop",
            NotValidatedAddress: "Test Address",
            UserId: userId,
            address: "Validated Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: photos,
            Schedules: new List<CoffeePeek.Contract.Dtos.Schedule.ScheduleDto>(),
            Latitude: 55.7558m,
            Longitude: 37.6173m
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

        var shopPhotos = await dbContext.ShopPhotos
            .Where(p => p.ShopId == shop!.Id)
            .ToListAsync();
        
        shopPhotos.Should().HaveCount(2);
        shopPhotos.Should().Contain(p => p.Url == "https://example.com/photo1.jpg");
        shopPhotos.Should().Contain(p => p.Url == "https://example.com/photo2.jpg");
        shopPhotos.All(p => p.UserId == userId).Should().BeTrue();
    }

    [Fact]
    public async Task CoffeeShopApprovedEventConsumer_WithoutCoordinates_DoesNotCreateLocation()
    {
        // Arrange
        await ClearDatabaseAsync();
        await EnsureTestCityAsync();

        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Coffee Shop",
            NotValidatedAddress: "Test Address",
            UserId: Guid.NewGuid(),
            address: null,
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: new List<string>(),
            Schedules: new List<CoffeePeek.Contract.Dtos.Schedule.ScheduleDto>(),
            Latitude: null,
            Longitude: null
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
        shop!.LocationId.Should().BeNull();
        
        var location = await dbContext.Locations.FirstOrDefaultAsync();
        location.Should().BeNull();
    }

    [Fact]
    public async Task CoffeeShopApprovedEventConsumer_WithCompleteData_CreatesAllEntities()
    {
        // Arrange
        await ClearDatabaseAsync();
        await EnsureTestCityAsync();

        var shopContact = new ShopContactDto
        {
            PhoneNumber = "+1234567890",
            InstagramLink = "https://instagram.com/test"
        };

        var photos = new List<string> { "https://example.com/photo1.jpg" };
        var userId = Guid.NewGuid();

        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Complete Test Shop",
            NotValidatedAddress: "Test Address",
            UserId: userId,
            address: "Validated Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: shopContact,
            ShopPhotos: photos,
            Schedules: new List<CoffeePeek.Contract.Dtos.Schedule.ScheduleDto>(),
            Latitude: 55.7558m,
            Longitude: 37.6173m
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
        shopPhotos.Should().HaveCount(1);
    }
}

