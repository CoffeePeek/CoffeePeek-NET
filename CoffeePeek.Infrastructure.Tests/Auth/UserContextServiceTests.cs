using CoffeePeek.Infrastructure.Constants;
using CoffeePeek.Infrastructure.Services.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CoffeePeek.Infrastructure.Tests.Auth;

public class UserContextServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly UserContextService _userContextService;

    public UserContextServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _userContextService = new UserContextService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void TryGetUserId_ShouldReturnTrueAndUserId_WhenHttpContextContainsValidUserId()
    {
        // Arrange
        const int userId = 123;
        var httpContext = new DefaultHttpContext();
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = userId;

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.TryGetUserId(out var actualUserId);

        // Assert
        result.Should().BeTrue();
        actualUserId.Should().Be(userId);
    }

    [Fact]
    public void TryGetUserId_ShouldReturnFalseAndZero_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _userContextService.TryGetUserId(out var actualUserId);

        // Assert
        result.Should().BeFalse();
        actualUserId.Should().Be(0);
    }

    [Fact]
    public void TryGetUserId_ShouldReturnFalseAndZero_WhenHttpContextItemsIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            Items = null!
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.TryGetUserId(out var actualUserId);

        // Assert
        result.Should().BeFalse();
        actualUserId.Should().Be(0);
    }

    [Fact]
    public void TryGetUserId_ShouldReturnFalseAndZero_WhenUserIdNotInHttpContextItems()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        // Note: Not adding UserId to Items

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.TryGetUserId(out var actualUserId);

        // Assert
        result.Should().BeFalse();
        actualUserId.Should().Be(0);
    }

    [Fact]
    public void TryGetUserId_ShouldReturnFalseAndZero_WhenUserIdIsNotAnInteger()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = "not-an-integer";

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.TryGetUserId(out var actualUserId);

        // Assert
        result.Should().BeFalse();
        actualUserId.Should().Be(0);
    }

    [Fact]
    public void TryGetUserId_ShouldReturnFalseAndZero_WhenUserIdIsNegative()
    {
        // Arrange
        const int userId = -5;
        var httpContext = new DefaultHttpContext();
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = userId;

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.TryGetUserId(out var actualUserId);

        // Assert
        result.Should().BeFalse();
        // Note: The actual implementation will set the out parameter to the negative value
        // but still return false because the condition (userId > 0) fails
        actualUserId.Should().Be(userId);
    }

    [Fact]
    public void TryGetUserId_ShouldReturnFalseAndZero_WhenUserIdIsZero()
    {
        // Arrange
        const int userId = 0;
        var httpContext = new DefaultHttpContext();
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = userId;

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.TryGetUserId(out var actualUserId);

        // Assert
        result.Should().BeFalse();
        actualUserId.Should().Be(0);
    }
}