using CoffeePeek.Api.Controllers;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Infrastructure.Constants;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoffeePeek.Api.Test.Controllers;

public class ReviewCoffeeControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ReviewCoffeeController _controller;

    public ReviewCoffeeControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ReviewCoffeeController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAllReviews_ShouldReturnReviews_WhenUserAuthenticated()
    {
        // Arrange
        var userId = 1;
        var expectedResponse = new Response<GetAllReviewsResponse>
        {
            Success = true,
            Data = new GetAllReviewsResponse([])
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllReviewsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var httpContext = CreateHttpContextWithUserId(userId);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.GetAllReviews();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetAllReviewsRequest>(r => r.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllReviews_ShouldThrowException_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        await _controller.Invoking(c => c.GetAllReviews())
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated or UserId is not available.");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetAllReviewsRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnReview_WhenReviewExists()
    {
        // Arrange
        var reviewId = 5;
        var userId = 1;
        var expectedResponse = new Response<GetReviewByIdResponse>
        {
            Success = true,
            Data = new GetReviewByIdResponse(new CoffeeShopReviewDto())
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var httpContext = CreateHttpContextWithUserId(userId);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.GetReviewById(reviewId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetReviewByIdRequest>(r => r.Id == reviewId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnNotFound_WhenReviewDoesNotExist()
    {
        // Arrange
        var reviewId = 999;
        var userId = 1;
        var expectedResponse = new Response<GetReviewByIdResponse>
        {
            Success = false,
            Message = "Review not found"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var httpContext = CreateHttpContextWithUserId(userId);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.GetReviewById(reviewId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Review not found");
    }

    [Fact]
    public async Task AddCoffeeShopReview_ShouldAddReview_WhenUserAuthenticated()
    {
        // Arrange
        var userId = 1;
        var request = new AddCoffeeShopReviewRequest();
        var expectedResponse = new Response<AddCoffeeShopReviewResponse>
        {
            Success = true,
            Data = new AddCoffeeShopReviewResponse(1)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddCoffeeShopReviewRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var httpContext = CreateHttpContextWithUserId(userId);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.AddCoffeeShopReview(request);

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
    public async Task AddCoffeeShopReview_ShouldThrowException_WhenUserNotAuthenticated()
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        await _controller.Invoking(c => c.AddCoffeeShopReview(request))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated or UserId is not available.");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<AddCoffeeShopReviewRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCoffeeShopReview_ShouldUpdateReview_WhenUserAuthenticated()
    {
        // Arrange
        var userId = 1;
        var request = new UpdateCoffeeShopReviewRequest();
        var expectedResponse = new Response<UpdateCoffeeShopReviewResponse>
        {
            Success = true,
            Data = new UpdateCoffeeShopReviewResponse(1)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateCoffeeShopReviewRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var httpContext = CreateHttpContextWithUserId(userId);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.UpdateCoffeeShopReview(request);

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
    public async Task UpdateCoffeeShopReview_ShouldThrowException_WhenUserNotAuthenticated()
    {
        // Arrange
        var request = new UpdateCoffeeShopReviewRequest();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        await _controller.Invoking(c => c.UpdateCoffeeShopReview(request))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated or UserId is not available.");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateCoffeeShopReviewRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddCoffeeShopReview_ShouldSetUserId_FromHttpContext()
    {
        // Arrange
        var userId = 42;
        var request = new AddCoffeeShopReviewRequest();
        var expectedResponse = new Response<AddCoffeeShopReviewResponse>
        {
            Success = true,
            Data = new AddCoffeeShopReviewResponse(1)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddCoffeeShopReviewRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var httpContext = CreateHttpContextWithUserId(userId);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        await _controller.AddCoffeeShopReview(request);

        // Assert
        request.UserId.Should().Be(userId);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<AddCoffeeShopReviewRequest>(r => r.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCoffeeShopReview_ShouldSetUserId_FromHttpContext()
    {
        // Arrange
        var userId = 42;
        var request = new UpdateCoffeeShopReviewRequest();
        var expectedResponse = new Response<UpdateCoffeeShopReviewResponse>
        {
            Success = true,
            Data = new UpdateCoffeeShopReviewResponse(1)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateCoffeeShopReviewRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var httpContext = CreateHttpContextWithUserId(userId);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        await _controller.UpdateCoffeeShopReview(request);

        // Assert
        request.UserId.Should().Be(userId);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<UpdateCoffeeShopReviewRequest>(r => r.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static HttpContext CreateHttpContextWithUserId(int userId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = userId;
        return httpContext;
    }
}