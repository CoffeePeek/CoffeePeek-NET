using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
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
        var originalEvent = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Coffee Shop",
            NotValidatedAddress: "123 Test Street",
            UserId: Guid.NewGuid(),
            address: "123 Test Street, Validated",
            ShopContactId: Guid.NewGuid(),
            Status: ShopStatus.NotConfirmed,
            ShopContact: new ShopContactDto
            {
                PhoneNumber = "+7-999-123-4567",
                InstagramLink = "https://instagram.com/testshop",
                Email = "test@example.com",
                SiteLink = "https://testshop.com"
            },
            ShopPhotos: new List<string> { "https://example.com/photo1.jpg", "https://example.com/photo2.jpg" },
            Schedules: new List<ScheduleDto>
            {
                new ScheduleDto
                {
                    DayOfWeek = DayOfWeek.Monday,
                    OpeningTime = TimeSpan.FromHours(9),
                    ClosingTime = TimeSpan.FromHours(18),
                    Intervals = new HashSet<ShopScheduleIntervalDto>()
                }
            },
            Latitude: 55.7558m,
            Longitude: 37.6173m
        );

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<CoffeeShopApprovedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.ModerationShopId.Should().Be(originalEvent.ModerationShopId);
        deserializedEvent.Name.Should().Be(originalEvent.Name);
        deserializedEvent.NotValidatedAddress.Should().Be(originalEvent.NotValidatedAddress);
        deserializedEvent.UserId.Should().Be(originalEvent.UserId);
        deserializedEvent.address.Should().Be(originalEvent.address);
        deserializedEvent.ShopContactId.Should().Be(originalEvent.ShopContactId);
        deserializedEvent.Status.Should().Be(originalEvent.Status);
        deserializedEvent.Latitude.Should().Be(originalEvent.Latitude);
        deserializedEvent.Longitude.Should().Be(originalEvent.Longitude);
        
        deserializedEvent.ShopContact.Should().NotBeNull();
        deserializedEvent.ShopContact!.PhoneNumber.Should().Be(originalEvent.ShopContact.PhoneNumber);
        deserializedEvent.ShopContact.InstagramLink.Should().Be(originalEvent.ShopContact.InstagramLink);
        deserializedEvent.ShopContact.Email.Should().Be(originalEvent.ShopContact.Email);
        deserializedEvent.ShopContact.SiteLink.Should().Be(originalEvent.ShopContact.SiteLink);
        
        deserializedEvent.ShopPhotos.Should().HaveCount(2);
        deserializedEvent.ShopPhotos.Should().Contain("https://example.com/photo1.jpg");
        deserializedEvent.ShopPhotos.Should().Contain("https://example.com/photo2.jpg");
        
        deserializedEvent.Schedules.Should().HaveCount(1);
        var schedule = deserializedEvent.Schedules.First();
        schedule.DayOfWeek.Should().Be(DayOfWeek.Monday);
        schedule.OpeningTime.Should().Be(TimeSpan.FromHours(9));
        schedule.ClosingTime.Should().Be(TimeSpan.FromHours(18));
    }

    [Fact]
    public void CoffeeShopApprovedEvent_WithNullOptionalFields_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var originalEvent = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Coffee Shop",
            NotValidatedAddress: "123 Test Street",
            UserId: Guid.NewGuid(),
            address: "123 Test Street",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: new List<string>(),
            Schedules: new List<ScheduleDto>(),
            Latitude: null,
            Longitude: null
        );

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<CoffeeShopApprovedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.ModerationShopId.Should().Be(originalEvent.ModerationShopId);
        deserializedEvent.Name.Should().Be(originalEvent.Name);
        deserializedEvent.ShopContactId.Should().BeNull();
        deserializedEvent.ShopContact.Should().BeNull();
        deserializedEvent.ShopPhotos.Should().BeEmpty();
        deserializedEvent.Schedules.Should().BeEmpty();
        deserializedEvent.Latitude.Should().BeNull();
        deserializedEvent.Longitude.Should().BeNull();
    }

    [Fact]
    public void CoffeeShopApprovedEvent_WithMinimalData_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var originalEvent = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Minimal Shop",
            NotValidatedAddress: "Address",
            UserId: Guid.NewGuid(),
            address: "Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: new List<string>(),
            Schedules: new List<ScheduleDto>(),
            Latitude: null,
            Longitude: null
        );

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<CoffeeShopApprovedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.Name.Should().Be("Minimal Shop");
        deserializedEvent.Status.Should().Be(ShopStatus.NotConfirmed);
    }

    [Fact]
    public void CoffeeShopApprovedEvent_JsonStructure_MatchesExpectedFormat()
    {
        // Arrange
        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.Parse("12345678-1234-1234-1234-123456789012"),
            Name: "Test Shop",
            NotValidatedAddress: "Test Address",
            UserId: Guid.Parse("87654321-4321-4321-4321-210987654321"),
            address: "Validated Address",
            ShopContactId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Status: ShopStatus.NotConfirmed,
            ShopContact: new ShopContactDto
            {
                PhoneNumber = "+7-999-123-4567"
            },
            ShopPhotos: new List<string> { "photo1.jpg" },
            Schedules: new List<ScheduleDto>(),
            Latitude: 55.7558m,
            Longitude: 37.6173m
        );

        // Act
        var json = JsonSerializer.Serialize(@event, _jsonOptions);

        // Assert
        json.Should().Contain("moderationShopId");
        json.Should().Contain("name");
        json.Should().Contain("notValidatedAddress");
        json.Should().Contain("userId");
        json.Should().Contain("address");
        json.Should().Contain("shopContactId");
        json.Should().Contain("status");
        json.Should().Contain("shopContact");
        json.Should().Contain("shopPhotos");
        json.Should().Contain("schedules");
        json.Should().Contain("latitude");
        json.Should().Contain("longitude");
        json.Should().Contain("12345678-1234-1234-1234-123456789012");
        json.Should().Contain("Test Shop");
        json.Should().Contain("55.7558");
        json.Should().Contain("37.6173");
    }

    [Fact]
    public void CoffeeShopApprovedEvent_AllShopStatusValues_CanBeSerialized()
    {
        // Arrange & Act & Assert
        foreach (ShopStatus status in Enum.GetValues<ShopStatus>())
        {
            var @event = new CoffeeShopApprovedEvent(
                ModerationShopId: Guid.NewGuid(),
                Name: "Test",
                NotValidatedAddress: "Address",
                UserId: Guid.NewGuid(),
                address: "Address",
                ShopContactId: null,
                Status: status,
                ShopContact: null,
                ShopPhotos: new List<string>(),
                Schedules: new List<ScheduleDto>(),
                Latitude: null,
                Longitude: null
            );

            var json = JsonSerializer.Serialize(@event, _jsonOptions);
            var deserialized = JsonSerializer.Deserialize<CoffeeShopApprovedEvent>(json, _jsonOptions);

            deserialized.Should().NotBeNull();
            deserialized!.Status.Should().Be(status);
        }
    }
}

