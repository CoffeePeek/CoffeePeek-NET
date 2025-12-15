using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace CoffeePeek.Contract.Tests.Events;

public class CoffeeShopApprovedEventContractTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    [Fact]
    public void CoffeeShopApprovedEvent_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var creatorId = Guid.NewGuid();

        var shop = new ShopDto
        {
            Id = Guid.NewGuid(),
            CityId = Guid.NewGuid(),
            Name = "Test Coffee Shop",
            Description = "Test description",
            PriceRange = PriceRange.Cheap,
            Location = new LocationDto
            {
                Address = "123 Test Street, Validated",
                Latitude = 55.7558m,
                Longitude = 37.6173m
            },
            ShopContact = new ShopContactDto
            {
                PhoneNumber = "+7-999-123-4567",
                InstagramLink = "https://instagram.com/testshop",
                Email = "test@example.com",
                SiteLink = "https://testshop.com"
            },
            Schedules = new List<ScheduleDto>
            {
                new()
                {
                    DayOfWeek = DayOfWeek.Monday,
                    OpeningTime = TimeSpan.FromHours(9),
                    ClosingTime = TimeSpan.FromHours(18),
                    Intervals = new HashSet<ShopScheduleIntervalDto>()
                }
            },
            ImageUrls = new[] { "https://example.com/photo1.jpg", "https://example.com/photo2.jpg" }
        };

        var originalEvent = new CoffeeShopApprovedEvent(creatorId, shop);

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<CoffeeShopApprovedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.CreatorId.Should().Be(originalEvent.CreatorId);
        deserializedEvent.Shop.Should().NotBeNull();

        deserializedEvent.Shop.Name.Should().Be(originalEvent.Shop.Name);
        deserializedEvent.Shop.Description.Should().Be(originalEvent.Shop.Description);

        deserializedEvent.Shop.Location.Should().NotBeNull();
        deserializedEvent.Shop.Location!.Address.Should().Be(originalEvent.Shop.Location!.Address);
        deserializedEvent.Shop.Location.Latitude.Should().Be(originalEvent.Shop.Location.Latitude);
        deserializedEvent.Shop.Location.Longitude.Should().Be(originalEvent.Shop.Location.Longitude);

        deserializedEvent.Shop.ShopContact.Should().NotBeNull();
        deserializedEvent.Shop.ShopContact!.PhoneNumber.Should().Be(originalEvent.Shop.ShopContact!.PhoneNumber);
        deserializedEvent.Shop.ShopContact.InstagramLink.Should().Be(originalEvent.Shop.ShopContact.InstagramLink);
        deserializedEvent.Shop.ShopContact.Email.Should().Be(originalEvent.Shop.ShopContact.Email);
        deserializedEvent.Shop.ShopContact.SiteLink.Should().Be(originalEvent.Shop.ShopContact.SiteLink);

        deserializedEvent.Shop.ImageUrls.Should().NotBeNull();
        deserializedEvent.Shop.ImageUrls!.Should().HaveCount(2);
        deserializedEvent.Shop.ImageUrls.Should().Contain("https://example.com/photo1.jpg");
        deserializedEvent.Shop.ImageUrls.Should().Contain("https://example.com/photo2.jpg");

        deserializedEvent.Shop.Schedules.Should().NotBeNull();
        deserializedEvent.Shop.Schedules!.Should().HaveCount(1);
        var schedule = deserializedEvent.Shop.Schedules.First();
        schedule.DayOfWeek.Should().Be(DayOfWeek.Monday);
        schedule.OpeningTime.Should().Be(TimeSpan.FromHours(9));
        schedule.ClosingTime.Should().Be(TimeSpan.FromHours(18));
    }

    [Fact]
    public void CoffeeShopApprovedEvent_WithNullOptionalFields_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var creatorId = Guid.NewGuid();

        var shop = new ShopDto
        {
            Id = Guid.NewGuid(),
            CityId = Guid.NewGuid(),
            Name = "Test Coffee Shop",
            Description = null,
            PriceRange = PriceRange.Cheap,
            Location = null,
            ShopContact = null,
            Schedules = null,
            ImageUrls = null
        };

        var originalEvent = new CoffeeShopApprovedEvent(creatorId, shop);

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<CoffeeShopApprovedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.CreatorId.Should().Be(originalEvent.CreatorId);
        deserializedEvent.Shop.Should().NotBeNull();
        deserializedEvent.Shop.Name.Should().Be(originalEvent.Shop.Name);
        deserializedEvent.Shop.Description.Should().BeNull();
        deserializedEvent.Shop.Location.Should().BeNull();
        deserializedEvent.Shop.ShopContact.Should().BeNull();
        deserializedEvent.Shop.Schedules.Should().BeNull();
        deserializedEvent.Shop.ImageUrls.Should().BeNull();
    }

    [Fact]
    public void CoffeeShopApprovedEvent_WithMinimalData_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var creatorId = Guid.NewGuid();

        var shop = new ShopDto
        {
            Id = Guid.NewGuid(),
            CityId = Guid.NewGuid(),
            Name = "Minimal Shop"
        };

        var originalEvent = new CoffeeShopApprovedEvent(creatorId, shop);

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<CoffeeShopApprovedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.CreatorId.Should().Be(originalEvent.CreatorId);
        deserializedEvent.Shop.Name.Should().Be("Minimal Shop");
    }

    [Fact]
    public void CoffeeShopApprovedEvent_JsonStructure_MatchesExpectedFormat()
    {
        // Arrange
        var creatorId = Guid.Parse("87654321-4321-4321-4321-210987654321");

        var shop = new ShopDto
        {
            Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            CityId = Guid.NewGuid(),
            Name = "Test Shop",
            Description = "Test Address",
            PriceRange = PriceRange.Cheap,
            Location = new LocationDto
            {
                Address = "Validated Address",
                Latitude = 55.7558m,
                Longitude = 37.6173m
            },
            ShopContact = new ShopContactDto
            {
                PhoneNumber = "+7-999-123-4567"
            },
            ImageUrls = new[] { "photo1.jpg" },
            Schedules = new List<ScheduleDto>()
        };

        var @event = new CoffeeShopApprovedEvent(creatorId, shop);

        // Act
        var json = JsonSerializer.Serialize(@event, _jsonOptions);

        // Assert
        json.Should().Contain("creatorId");
        json.Should().Contain("shop");
        json.Should().Contain("id");
        json.Should().Contain("name");
        json.Should().Contain("description");
        json.Should().Contain("priceRange");
        json.Should().Contain("location");
        json.Should().Contain("address");
        json.Should().Contain("latitude");
        json.Should().Contain("longitude");
        json.Should().Contain("shopContact");
        json.Should().Contain("imageUrls");
        json.Should().Contain("schedules");
        json.Should().Contain("12345678-1234-1234-1234-123456789012");
        json.Should().Contain("Test Shop");
        json.Should().Contain("55.7558");
        json.Should().Contain("37.6173");
    }
}

