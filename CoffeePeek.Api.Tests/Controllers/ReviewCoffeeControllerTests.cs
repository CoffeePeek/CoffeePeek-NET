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
using System.Security.Claims;
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

        var httpContext = CreateHttpContextWithAuthenticatedUser(userId);
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
            .WithMessage("User ID claim is missing.");

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

        // Note: GetReviewById doesn't require user authentication in the method itself,
        // but the controller has [Authorize] which would handle this at the HTTP level
        var httpContext = new DefaultHttpContext();
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
        var expectedResponse = new Response<GetReviewByIdResponse>
        {
            Success = false,
            Message = "Review not found"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Note: GetReviewById doesn't require user authentication in the method itself,
        // but the controller has [Authorize] which would handle this at the HTTP level
        var httpContext = new DefaultHttpContext();
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
    public async Task GetReviewById_ShouldReturnReview_WhenReviewExistsWithValidData()
    {
        // Arrange
        var reviewId = 5;
        var reviewDto = new CoffeeShopReviewDto
        {
            Id = reviewId,
            ShopId = 10,
            UserId = 1,
            Header = "Great coffee!",
            Comment = "This coffee shop has excellent coffee and great service.",
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 4,
            CreatedAt = DateTime.UtcNow,
            ShopName = "Best Coffee Shop"
        };
        var expectedResponse = new Response<GetReviewByIdResponse>
        {
            Success = true,
            Data = new GetReviewByIdResponse(reviewDto)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Note: GetReviewById doesn't require user authentication in the method itself,
        // but the controller has [Authorize] which would handle this at the HTTP level
        var httpContext = new DefaultHttpContext();
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
        result.Data!.Review.Should().NotBeNull();
        result.Data.Review.Id.Should().Be(reviewId);
        result.Data.Review.ShopId.Should().Be(10);
        result.Data.Review.UserId.Should().Be(1);
        result.Data.Review.Header.Should().Be("Great coffee!");
        result.Data.Review.Comment.Should().Be("This coffee shop has excellent coffee and great service.");
        result.Data.Review.RatingCoffee.Should().Be(5);
        result.Data.Review.RatingService.Should().Be(4);
        result.Data.Review.RatingPlace.Should().Be(4);
        result.Data.Review.ShopName.Should().Be("Best Coffee Shop");

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetReviewByIdRequest>(r => r.Id == reviewId),
                It.IsAny<CancellationToken>()),
            Times.Once);
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

        var httpContext = CreateHttpContextWithAuthenticatedUser(userId);
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
            .WithMessage("User ID claim is missing.");

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

        var httpContext = CreateHttpContextWithAuthenticatedUser(userId);
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
            .WithMessage("User ID claim is missing.");

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

        var httpContext = CreateHttpContextWithAuthenticatedUser(userId);
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

        var httpContext = CreateHttpContextWithAuthenticatedUser(userId);
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

    private static HttpContext CreateHttpContextWithAuthenticatedUser(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, $"testuser{userId}")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        return httpContext;
    }
}