using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.WebClient;
using CoffeePeek.Contract.Dtos;
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
                Data = new GetAllModerationShopsResultDto { ModerationShops = [shop] }
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
    public async Task UpdateReviewStatusAsync_PutBodyToModerationReviews()
    {
        var id = Guid.NewGuid();
        HttpCommand? captured = null;
        _executorMock
            .Setup(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((c, _) => captured = c)
            .ReturnsAsync(new ApiResponse { IsSuccess = true, StatusCode = 200 });

        var sut = CreateSut();
        var r = await sut.UpdateReviewStatusAsync(id, ModerationStatus.Rejected, "bad");

        r.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Endpoint.Should().Be("api/ModerationReviews");
        captured.Method.Should().Be(HttpMethod.Put);
        var body = (ChangeModerationReviewStatusRequest)captured.Body!;
        body.ModerationReviewId.Should().Be(id);
        body.ModerationStatus.Should().Be(ModerationStatus.Rejected);
        body.RejectReason.Should().Be("bad");
    }

    [Fact]
    public async Task GetAllReviewsAsync_DeserializesReviewDtos()
    {
        var review = new ModerationReviewDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserName = "u",
            Header = "h",
            Comment = "c",
            ShopId = Guid.NewGuid(),
            Rating = new RatingDto { Place = 5, Service = 5, Coffee = 5 },
            CreatedAt = DateTime.UtcNow,
            ModerationStatus = ModerationStatus.Pending
        };
        _executorMock
            .Setup(e => e.Execute<GetAllModerationReviewsResultDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetAllModerationReviewsResultDto>
            {
                IsSuccess = true,
                StatusCode = 200,
                Data = new GetAllModerationReviewsResultDto { ReviewDtos = [review] }
            });

        var sut = CreateSut();
        var r = await sut.GetAllReviewsAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().ContainSingle();
        r.Value[0].Header.Should().Be("h");
    }
}
