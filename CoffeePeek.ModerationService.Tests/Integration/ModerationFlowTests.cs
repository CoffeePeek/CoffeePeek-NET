using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.ModerationService.Configuration;
using CoffeePeek.ModerationService.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CoffeePeek.ModerationService.Tests.Integration;

public class ModerationFlowTests(ModerationServiceWebApplicationFactory factory)
    : IClassFixture<ModerationServiceWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostModeration_CreatesModerationShop()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SendCoffeeShopToModerationRequest
        {
            Name = $"Test Shop {Guid.NewGuid()}",
            NotValidatedAddress = "Test Address",
            UserId = userId
        };

        // Reset mock and setup Yandex Geocoding mock
        factory.YandexGeocodingServiceMock.Reset();
        factory.YandexGeocodingServiceMock
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodingResult(55.7558m, 37.6173m));

        // Act
        var response = await _client.PostAsJsonAsync("/api/moderation", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify in database
        // Note: Controller overrides UserId with authenticated user's ID, so query by Name only
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ModerationDbContext>();
        var shop = await dbContext.ModerationShops
            .FirstOrDefaultAsync(s => s.Name == request.Name);

        shop.Should().NotBeNull();
        shop!.Name.Should().Be(request.Name);
        shop.NotValidatedAddress.Should().Be(request.NotValidatedAddress);
    }

    [Fact]
    public async Task PostModeration_WithGeocoding_SavesCoordinates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SendCoffeeShopToModerationRequest
        {
            Name = $"Test Shop {Guid.NewGuid()}",
            NotValidatedAddress = "Moscow, Red Square",
            UserId = userId
        };

        var latitude = 55.7558m;
        var longitude = 37.6173m;

        // Reset mock and setup Yandex Geocoding mock
        factory.YandexGeocodingServiceMock.Reset();
        factory.YandexGeocodingServiceMock
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodingResult(latitude, longitude));

        // Act
        var response = await _client.PostAsJsonAsync("/api/moderation", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ModerationDbContext>();
        // Note: Controller overrides UserId with authenticated user's ID, so query by Name only
        var shop = await dbContext.ModerationShops
            .FirstOrDefaultAsync(s => s.Name == request.Name);

        shop.Should().NotBeNull();
        shop!.Latitude.Should().Be(latitude);
        shop.Longitude.Should().Be(longitude);
        shop.IsAddressValidated.Should().BeTrue();
    }

    [Fact]
    public async Task PostModeration_WithGeocodingFailure_SetsFlag()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SendCoffeeShopToModerationRequest
        {
            Name = $"Test Shop {Guid.NewGuid()}",
            NotValidatedAddress = "Invalid Address 12345",
            UserId = userId
        };

        // Reset mock and setup Yandex Geocoding mock
        factory.YandexGeocodingServiceMock.Reset();
        factory.YandexGeocodingServiceMock
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeocodingResult?)null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/moderation", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ModerationDbContext>();
        // Note: Controller overrides UserId with authenticated user's ID, so query by Name only
        var shop = await dbContext.ModerationShops
            .FirstOrDefaultAsync(s => s.Name == request.Name);

        shop.Should().NotBeNull();
        shop!.IsAddressValidated.Should().BeFalse();
        shop.Latitude.Should().BeNull();
        shop.Longitude.Should().BeNull();
    }

    [Fact]
    public async Task FullModerationFlow_CreateGetApprove_PublishesEvent()
    {
        // Arrange
        var shopName = $"Flow{Guid.NewGuid():N}".Substring(0, Math.Min(45, $"Flow{Guid.NewGuid():N}".Length)); // Ensure max 50 chars
        var address = "Moscow, Red Square";
        var latitude = 55.7558m;
        var longitude = 37.6173m;

        // Reset mocks
        factory.YandexGeocodingServiceMock.Reset();
        factory.PublishEndpointMock.Reset();

        factory.YandexGeocodingServiceMock
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodingResult(latitude, longitude));

        // Step 1: Create moderation
        var createRequest = new SendCoffeeShopToModerationRequest
        {
            Name = shopName,
            NotValidatedAddress = address,
            UserId = Guid.NewGuid()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/moderation", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 2: Get moderation shops list
        var getResponse = await _client.GetAsync("/api/moderation");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var moderationShops = await getResponse.Content.ReadFromJsonAsync<CoffeePeek.Contract.Responses.Response<GetCoffeeShopsInModerationByIdResponse>>();
        moderationShops.Should().NotBeNull();
        moderationShops!.IsSuccess.Should().BeTrue();
        moderationShops.Data.Should().NotBeNull();
        moderationShops.Data!.ModerationShop.Should().Contain(s => s.Name == shopName);

        var moderationShop = moderationShops.Data.ModerationShop.First(s => s.Name == shopName);

        // Step 3: Approve moderation
        var approveResponse = await _client.PutAsync(
            $"/api/moderation/status?id={moderationShop.Id}&status={ModerationStatus.Approved}",
            null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Verify event was published
        factory.PublishEndpointMock.Verify(
            x => x.Publish(
                It.Is<CoffeeShopApprovedEvent>(e =>
                    e.ModerationShopId == moderationShop.Id &&
                    e.Name == shopName &&
                    e.NotValidatedAddress == address &&
                    e.Latitude == latitude &&
                    e.Longitude == longitude),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Step 5: Verify status was updated in database
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ModerationDbContext>();
        var updatedShop = await dbContext.ModerationShops.FindAsync(moderationShop.Id);
        updatedShop.Should().NotBeNull();
        updatedShop!.ModerationStatus.Should().Be(ModerationStatus.Approved);
    }

    [Fact]
    public async Task FullModerationFlow_WithContactAndPhotos_PublishesCompleteEvent()
    {
        // Arrange
        var guidStr = Guid.NewGuid().ToString("N");
        var shopName = $"Complete{guidStr}".Substring(0, Math.Min(45, $"Complete{guidStr}".Length)); // Ensure max 50 chars
        var address = "St. Petersburg, Nevsky Prospect";
        var latitude = 59.9343m;
        var longitude = 30.3351m;
        var phoneNumber = "+7-999-123-4567";
        var instagramLink = "https://instagram.com/testshop";
        var photoUrl = "https://example.com/photo.jpg";

        // Reset mocks
        factory.YandexGeocodingServiceMock.Reset();
        factory.PublishEndpointMock.Reset();

        factory.YandexGeocodingServiceMock
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodingResult(latitude, longitude));

        // Step 1: Create moderation
        var createRequest = new SendCoffeeShopToModerationRequest
        {
            Name = shopName,
            NotValidatedAddress = address,
            UserId = Guid.NewGuid()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/moderation", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get created shop ID
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ModerationDbContext>();
        var shop = await dbContext.ModerationShops.FirstOrDefaultAsync(s => s.Name == shopName);
        shop.Should().NotBeNull();

        // Add contact and photo to shop (simulating update)
        var contact = new CoffeePeek.ModerationService.Models.ShopContacts
        {
            Id = Guid.NewGuid(),
            ShopId = shop!.Id,
            PhoneNumber = phoneNumber,
            InstagramLink = instagramLink
        };
        dbContext.ShopContacts.Add(contact);
        shop.ShopContactId = contact.Id;

        var photo = new CoffeePeek.ModerationService.Models.ShopPhoto
        {
            Id = Guid.NewGuid(),
            ShopId = shop.Id,
            Url = photoUrl,
            UserId = shop.UserId
        };
        dbContext.ShopPhotos.Add(photo);
        await dbContext.SaveChangesAsync();

        // Step 2: Approve moderation
        var approveResponse = await _client.PutAsync(
            $"/api/moderation/status?id={shop.Id}&status={ModerationStatus.Approved}",
            null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Verify event was published with all data
        factory.PublishEndpointMock.Verify(
            x => x.Publish(
                It.Is<CoffeeShopApprovedEvent>(e =>
                    e.ModerationShopId == shop.Id &&
                    e.Name == shopName &&
                    e.NotValidatedAddress == address &&
                    e.Latitude == latitude &&
                    e.Longitude == longitude &&
                    e.ShopContact != null &&
                    e.ShopContact.PhoneNumber == phoneNumber &&
                    e.ShopContact.InstagramLink == instagramLink &&
                    e.ShopPhotos != null &&
                    e.ShopPhotos.Contains(photoUrl)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FullModerationFlow_ApproveNonExistentShop_ReturnsError()
    {
        // Arrange
        factory.PublishEndpointMock.Reset();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync(
            $"/api/moderation/status?id={nonExistentId}&status={ModerationStatus.Approved}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Controller returns 200 with error in body
        var result = await response.Content.ReadFromJsonAsync<CoffeePeek.Contract.Response.Response>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("CoffeeShop not found");

        // Verify no event was published
        factory.PublishEndpointMock.Verify(
            x => x.Publish(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FullModerationFlow_GetModerationShops_ReturnsUserShops()
    {
        // Arrange
        var guid1 = Guid.NewGuid().ToString("N");
        var guid2 = Guid.NewGuid().ToString("N");
        var shopName1 = $"Shop1{guid1}".Substring(0, Math.Min(45, $"Shop1{guid1}".Length)); // Ensure max 50 chars
        var shopName2 = $"Shop2{guid2}".Substring(0, Math.Min(45, $"Shop2{guid2}".Length)); // Ensure max 50 chars

        factory.YandexGeocodingServiceMock.Reset();
        factory.YandexGeocodingServiceMock
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodingResult(55.7558m, 37.6173m));

        // Create two shops (controller will override UserId with authenticated user's ID)
        var request1 = new SendCoffeeShopToModerationRequest
        {
            Name = shopName1,
            NotValidatedAddress = "Address 1",
            UserId = Guid.NewGuid() // Will be overridden by controller
        };
        var request2 = new SendCoffeeShopToModerationRequest
        {
            Name = shopName2,
            NotValidatedAddress = "Address 2",
            UserId = Guid.NewGuid() // Will be overridden by controller
        };

        await _client.PostAsJsonAsync("/api/moderation", request1);
        await _client.PostAsJsonAsync("/api/moderation", request2);

        // Act
        var response = await _client.GetAsync("/api/moderation");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CoffeePeek.Contract.Responses.Response<GetCoffeeShopsInModerationByIdResponse>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ModerationShop.Should().Contain(s => s.Name == shopName1);
        result.Data.ModerationShop.Should().Contain(s => s.Name == shopName2);
    }
}