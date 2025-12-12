using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.ModerationService.Configuration;
using CoffeePeek.ModerationService.Models;
using CoffeePeek.ModerationService.Services.Interfaces;
using CoffeePeek.ModerationService.Tests.Integration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CoffeePeek.E2ETests;

public class ModerationE2ETests(GatewayWebApplicationFactory gatewayFactory)
    : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly GatewayWebApplicationFactory _gatewayFactory = gatewayFactory;
    private readonly HttpClient _gatewayClient = gatewayFactory.CreateClient();
    private ModerationServiceWebApplicationFactory? _moderationServiceFactory;

    [Fact]
    public async Task E2E_ModerationFlow_CreateGetApprove_ThroughGateway()
    {
        // Arrange - Start ModerationService
        _moderationServiceFactory = new ModerationServiceWebApplicationFactory();
        await _moderationServiceFactory.InitializeAsync();

        try
        {
            var moderationClient = _moderationServiceFactory.CreateClient();
            var moderationBaseAddress = moderationClient.BaseAddress?.ToString() ?? "http://localhost:5004";
            var moderationUri = new Uri(moderationBaseAddress);

            // Configure Gateway to route to ModerationService
            Environment.SetEnvironmentVariable("MODERATION_HOST", moderationUri.Host);
            Environment.SetEnvironmentVariable("MODERATION_PORT", moderationUri.Port.ToString());

            // Setup mocks
            _moderationServiceFactory.YandexGeocodingServiceMock.Reset();
            _moderationServiceFactory.PublishEndpointMock.Reset();

            var latitude = 55.7558m;
            var longitude = 37.6173m;
            _moderationServiceFactory.YandexGeocodingServiceMock
                .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GeocodingResult(latitude, longitude));

            var shopName = $"E2E{Guid.NewGuid():N}".Substring(0, Math.Min(45, $"E2E{Guid.NewGuid():N}".Length));
            var address = "Moscow, Red Square";

            // Step 1: Create moderation through Gateway
            var createRequest = new SendCoffeeShopToModerationRequest
            {
                Name = shopName,
                NotValidatedAddress = address,
                UserId = Guid.NewGuid() // Will be overridden by controller
            };

            var createResponse = await _gatewayClient.PostAsJsonAsync("/api/moderation", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 2: Get moderation shops through Gateway
            var getResponse = await _gatewayClient.GetAsync("/api/moderation");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var moderationShops = await getResponse.Content.ReadFromJsonAsync<Response<GetCoffeeShopsInModerationByIdResponse>>();
            moderationShops.Should().NotBeNull();
            moderationShops!.IsSuccess.Should().BeTrue();
            moderationShops.Data.Should().NotBeNull();
            moderationShops.Data!.ModerationShop.Should().Contain(s => s.Name == shopName);

            var moderationShop = moderationShops.Data.ModerationShop.First(s => s.Name == shopName);

            // Step 3: Approve moderation through Gateway
            var approveResponse = await _gatewayClient.PutAsync(
                $"/api/moderation/status?id={moderationShop.Id}&status={ModerationStatus.Approved}",
                null);
            approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 4: Verify event was published
            _moderationServiceFactory.PublishEndpointMock.Verify(
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
            using var scope = _moderationServiceFactory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ModerationDbContext>();
            var updatedShop = await dbContext.ModerationShops.FindAsync(moderationShop.Id);
            updatedShop.Should().NotBeNull();
            updatedShop!.ModerationStatus.Should().Be(ModerationStatus.Approved);
        }
        finally
        {
            if (_moderationServiceFactory != null)
            {
                await _moderationServiceFactory.DisposeAsync();
            }
        }
    }
}

