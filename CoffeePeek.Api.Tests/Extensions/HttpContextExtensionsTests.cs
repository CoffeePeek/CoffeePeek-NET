using CoffeePeek.Api.Extensions;
using CoffeePeek.Infrastructure.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Api.Test.Extensions;

public class HttpContextExtensionsTests
{
    [Fact]
    public void GetUserId_ShouldReturnUserId_WhenUserIdExistsInItems()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var expectedUserId = 123;
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = expectedUserId;

        // Act
        var userId = httpContext.GetUserId();

        // Assert
        userId.Should().NotBeNull();
        userId.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenUserIdDoesNotExist()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        var userId = httpContext.GetUserId();

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenUserIdIsNotInt()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = "not-an-int";

        // Act
        var userId = httpContext.GetUserId();

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GetUserIdOrThrow_ShouldReturnUserId_WhenUserIdExists()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var expectedUserId = 456;
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = expectedUserId;

        // Act
        var userId = httpContext.GetUserIdOrThrow();

        // Assert
        userId.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetUserIdOrThrow_ShouldThrowException_WhenUserIdDoesNotExist()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act & Assert
        var action = () => httpContext.GetUserIdOrThrow();

        action.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated or UserId is not available.");
    }

    [Fact]
    public void GetUserIdOrThrow_ShouldThrowException_WhenUserIdIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = null;

        // Act & Assert
        var action = () => httpContext.GetUserIdOrThrow();

        action.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated or UserId is not available.");
    }

    [Fact]
    public void GetUserIdOrThrow_ShouldThrowException_WhenUserIdIsNotInt()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = "invalid-value";

        // Act & Assert
        var action = () => httpContext.GetUserIdOrThrow();

        action.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated or UserId is not available.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999)]
    [InlineData(int.MaxValue)]
    public void GetUserIdOrThrow_ShouldReturnCorrectUserId_ForVariousValidValues(int expectedUserId)
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[AuthConfig.JWTTokenUserPropertyName] = expectedUserId;

        // Act
        var userId = httpContext.GetUserIdOrThrow();

        // Assert
        userId.Should().Be(expectedUserId);
    }
}
