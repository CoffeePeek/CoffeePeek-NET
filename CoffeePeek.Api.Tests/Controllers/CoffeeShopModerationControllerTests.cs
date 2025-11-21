using CoffeePeek.Api.Controllers;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Domain.Enums.Shop;
using CoffeePeek.Infrastructure.Services.Auth.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace CoffeePeek.Api.Test.Controllers;

public class CoffeeShopModerationControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IUserContextService> _userContextServiceMock;
    private readonly CoffeeShopModerationController _controller;

    public CoffeeShopModerationControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _userContextServiceMock = new Mock<IUserContextService>();
        _controller = new CoffeeShopModerationController(_mediatorMock.Object, _userContextServiceMock.Object);
    }

    [Fact]
    public async Task GetCoffeeShopsInModerationByUserId_ShouldReturnShops_WhenUserIdValid()
    {
        // Arrange
        var userId = 1;
        var expectedResponse = new Response<GetCoffeeShopsInModerationByIdResponse>
        {
            Success = true,
            Data = new GetCoffeeShopsInModerationByIdResponse([])
        };

        _userContextServiceMock
            .Setup(s => s.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) =>
            {
                id = userId;
                return true;
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCoffeeShopsInModerationByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCoffeeShopsInModerationByUserId();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetCoffeeShopsInModerationByIdRequest>(r => r.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCoffeeShopsInModerationByUserId_ShouldReturnError_WhenUserIdNotFound()
    {
        // Arrange
        _userContextServiceMock
            .Setup(s => s.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns(false);

        // Act
        var result = await _controller.GetCoffeeShopsInModerationByUserId();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User ID not found or invalid.");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetCoffeeShopsInModerationByIdRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllModerationShops_ShouldReturnAllShops()
    {
        // Arrange
        var expectedResponse = new Response<GetCoffeeShopsInModerationByIdResponse>
        {
            Success = true,
            Data = new GetCoffeeShopsInModerationByIdResponse([])
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllModerationShopsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllModerationShops();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetAllModerationShopsRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendCoffeeShopToModeration_ShouldSendShop_WhenUserIdValid()
    {
        // Arrange
        var userId = 1;
        var request = new SendCoffeeShopToModerationRequest();
        var expectedResponse = new Response<SendCoffeeShopToModerationResponse>
        {
            Success = true,
            Data = new SendCoffeeShopToModerationResponse()
        };

        _userContextServiceMock
            .Setup(s => s.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) =>
            {
                id = userId;
                return true;
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SendCoffeeShopToModerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SendCoffeeShopToModeration(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        request.UserId.Should().Be(userId);

        _mediatorMock.Verify(
            m => m.Send(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendCoffeeShopToModeration_ShouldReturnError_WhenUserIdNotFound()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationRequest();

        _userContextServiceMock
            .Setup(s => s.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns(false);

        // Act
        var result = await _controller.SendCoffeeShopToModeration(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User ID not found or invalid.");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<SendCoffeeShopToModerationRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateModerationCoffeeShop_ShouldUpdateShop_WhenUserIdValid()
    {
        // Arrange
        var userId = 1;
        var request = new UpdateModerationCoffeeShopRequest();
        var expectedResponse = new Response<UpdateModerationCoffeeShopResponse>
        {
            Success = true,
            Data = new UpdateModerationCoffeeShopResponse()
        };

        _userContextServiceMock
            .Setup(s => s.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) =>
            {
                id = userId;
                return true;
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateModerationCoffeeShopRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateModerationCoffeeShop(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        request.UserId.Should().Be(userId);

        _mediatorMock.Verify(
            m => m.Send(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateModerationCoffeeShop_ShouldReturnError_WhenUserIdNotFound()
    {
        // Arrange
        var request = new UpdateModerationCoffeeShopRequest();

        _userContextServiceMock
            .Setup(s => s.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns(false);

        // Act
        var result = await _controller.UpdateModerationCoffeeShop(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User ID not found or invalid.");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateModerationCoffeeShopRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateModerationCoffeeShopStatus_ShouldUpdateStatus_WhenUserIdValid()
    {
        // Arrange
        var userId = 1;
        var shopId = 5;
        var status = ModerationStatus.Approved;
        var expectedResponse = new Response
        {
            Success = true
        };

        _userContextServiceMock
            .Setup(s => s.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) =>
            {
                id = userId;
                return true;
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateModerationCoffeeShopStatusRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateModerationCoffeeShopStatus(shopId, status);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<UpdateModerationCoffeeShopStatusRequest>(r =>
                    r.Id == shopId &&
                    r.ModerationStatus == status &&
                    r.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateModerationCoffeeShopStatus_ShouldReturnError_WhenUserIdNotFound()
    {
        // Arrange
        var shopId = 5;
        var status = ModerationStatus.Rejected;

        _userContextServiceMock
            .Setup(s => s.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns(false);

        // Act
        var result = await _controller.UpdateModerationCoffeeShopStatus(shopId, status);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User ID not found or invalid.");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateModerationCoffeeShopStatusRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(ModerationStatus.Pending)]
    [InlineData(ModerationStatus.Approved)]
    [InlineData(ModerationStatus.Rejected)]
    public async Task UpdateModerationCoffeeShopStatus_ShouldHandleDifferentStatuses(ModerationStatus status)
    {
        // Arrange
        var userId = 1;
        var shopId = 5;
        var expectedResponse = new Response { Success = true };

        _userContextServiceMock
            .Setup(s => s.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) =>
            {
                id = userId;
                return true;
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateModerationCoffeeShopStatusRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateModerationCoffeeShopStatus(shopId, status);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<UpdateModerationCoffeeShopStatusRequest>(r => r.ModerationStatus == status),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}