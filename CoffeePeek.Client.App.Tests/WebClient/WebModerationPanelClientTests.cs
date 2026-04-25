using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.WebClient;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Client.App.Tests.WebClient;

public class WebModerationPanelClientTests
{
    private readonly Mock<IHttpCommandExecutor> _executorMock = new();

    private WebModerationPanelClient CreateSut() => new(_executorMock.Object);

    [Fact]
    public async Task GetAllShopsAsync_UsesGetModerationShops()
    {
        var shop = new ModerationShopDto
        {
            Id = Guid.NewGuid(),
            Name = "A",
            Schedules = [],
            EquipmentIds = [],
            CoffeeBeanIds = [],
            RoasterIds = [],
            BrewMethodIds = [],
            ShopPhotos = []
        };
        _executorMock
            .Setup(e => e.Execute<GetAllModerationShopsResultDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetAllModerationShopsResultDto>
            {
                IsSuccess = true,
                StatusCode = 200,
                Data = new GetAllModerationShopsResultDto { ModerationShop = [shop] }
            });

        var sut = CreateSut();
        var r = await sut.GetAllShopsAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().ContainSingle();
        r.Value[0].Name.Should().Be("A");
        _executorMock.Verify(
            e => e.Execute<GetAllModerationShopsResultDto>(
                It.Is<HttpCommand>(c => c.Endpoint == "api/ModerationShops" && c.Method == HttpMethod.Get),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateShopStatusAsync_PutWithStatusQuery()
    {
        var id = Guid.NewGuid();
        _executorMock
            .Setup(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse { IsSuccess = true, StatusCode = 200 });

        var sut = CreateSut();
        var r = await sut.UpdateShopStatusAsync(id, ModerationStatus.Approved);

        r.IsSuccess.Should().BeTrue();
        _executorMock.Verify(
            e => e.Execute(
                It.Is<HttpCommand>(c =>
                    c.Method == HttpMethod.Put
                    && c.Endpoint == "api/ModerationShops/status"
                    && c.Query["id"] == id.ToString("D")
                    && c.Query["status"] == "Approved"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeReviewStatusAsync_PutBodyToModerationReviews()
    {
        var id = Guid.NewGuid();
        HttpCommand? captured = null;
        _executorMock
            .Setup(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((c, _) => captured = c)
            .ReturnsAsync(new ApiResponse { IsSuccess = true, StatusCode = 200 });

        var sut = CreateSut();
        var r = await sut.ChangeReviewStatusAsync(id, ModerationStatus.Rejected, "bad");

        r.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Endpoint.Should().Be("api/ModerationReviews");
        captured.Method.Should().Be(HttpMethod.Put);
    }
}
