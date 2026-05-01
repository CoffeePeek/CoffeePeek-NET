using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Reviews;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Infrastructure.WebClient;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Client.App.Tests.WebClient;

public class WebCoffeeShopReviewsClientTests
{
    private readonly Mock<IHttpCommandExecutor> _executorMock = new();

    private WebCoffeeShopReviewsClient CreateSut() => new(_executorMock.Object);

    [Fact]
    public async Task CanCreateAsync_SendsExpectedRequest()
    {
        var shopId = Guid.NewGuid();
        HttpCommand? captured = null;
        _executorMock
            .Setup(e => e.Execute<CanCreateCoffeeShopReviewResultDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((cmd, _) => captured = cmd)
            .ReturnsAsync(new ApiResponse<CanCreateCoffeeShopReviewResultDto>
            {
                IsSuccess = true,
                StatusCode = 200,
                Data = new CanCreateCoffeeShopReviewResultDto { CanCreate = true }
            });

        var sut = CreateSut();
        var result = await sut.CanCreateAsync(shopId);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Endpoint.Should().Be("api/CoffeeShopReviews/can-create");
        captured.Query["shopId"].Should().Be(shopId.ToString("D"));
        captured.Method.Should().Be(HttpMethod.Get);
        captured.IsAuthorize.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_SendsExpectedPayload()
    {
        HttpCommand? captured = null;
        _executorMock
            .Setup(e => e.Execute<CreateCoffeeShopReviewResultDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((cmd, _) => captured = cmd)
            .ReturnsAsync(new ApiResponse<CreateCoffeeShopReviewResultDto>
            {
                IsSuccess = true,
                StatusCode = 200,
                Data = new CreateCoffeeShopReviewResultDto { CheckInId = Guid.NewGuid(), ReviewId = Guid.NewGuid() }
            });

        var sut = CreateSut();
        var shopId = Guid.NewGuid();

        var result = await sut.CreateAsync(shopId, new CreateCoffeeShopReviewInput(5, 4, 5, "Great"));

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Endpoint.Should().Be("api/CheckIns");
        captured.Method.Should().Be(HttpMethod.Post);
        captured.IsAuthorize.Should().BeTrue();
        captured.Body.Should().BeOfType<CreateCoffeeShopReviewRequest>();
        captured.Body.Should().BeEquivalentTo(
            new CreateCoffeeShopReviewRequest
            {
                CoffeeShopId = shopId,
                IsPublic = true,
                VisitedAt = default,
                Note = "Great",
                Rating = new CoffeePeek.Contract.Dtos.RatingDto
                {
                    Place = 5,
                    Service = 4,
                    Coffee = 5
                }
            },
            options => options.Excluding(r => r.VisitedAt));
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteRequest()
    {
        var reviewId = Guid.NewGuid();
        HttpCommand? captured = null;
        _executorMock
            .Setup(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((cmd, _) => captured = cmd)
            .ReturnsAsync(new ApiResponse { IsSuccess = true, StatusCode = 204 });

        var sut = CreateSut();
        var result = await sut.DeleteAsync(reviewId);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Endpoint.Should().Be($"api/CoffeeShopReviews/{reviewId:D}");
        captured.Method.Should().Be(HttpMethod.Delete);
        captured.IsAuthorize.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WhenScoreOutOfRange_ReturnsFailureWithoutSendingRequest()
    {
        var sut = CreateSut();

        var result = await sut.CreateAsync(Guid.NewGuid(), new CreateCoffeeShopReviewInput(0, 4, 5, "Great"));

        result.IsFailed.Should().BeTrue();
        _executorMock.Verify(
            e => e.Execute<CreateCoffeeShopReviewResultDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
