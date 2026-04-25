using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
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

        var result = await sut.CreateAsync(shopId, "Great", 5, 4, 5);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Endpoint.Should().Be("api/CheckIns");
        captured.Method.Should().Be(HttpMethod.Post);
        captured.IsAuthorize.Should().BeTrue();
        captured.Body.Should().NotBeNull();
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
    }
}
