using System.Security.Claims;
using CoffeePeek.Api.Middleware;
using CoffeePeek.Infrastructure.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoffeePeek.Api.Test.Middleware;

public class UserTokenMiddlewareTests
{
    private readonly Mock<ILogger<UserTokenMiddleware>> _loggerMock;
    private readonly UserTokenMiddleware _middleware;
    private bool _nextCalled;

    public UserTokenMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<UserTokenMiddleware>>();
        _nextCalled = false;
        _middleware = new UserTokenMiddleware(
            next: _ =>
            {
                _nextCalled = true;
                return Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );
    }

    [Fact]
    public async Task InvokeAsync_ShouldExtractUserId_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = 123;
        var context = CreateHttpContextWithAuthenticatedUser(userId);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Items.Should().ContainKey(AuthConfig.JWTTokenUserPropertyName);
        context.Items[AuthConfig.JWTTokenUserPropertyName].Should().Be(userId);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Extracted User ID: {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotExtractUserId_WhenUserNotAuthenticated()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Items.Should().NotContainKey(AuthConfig.JWTTokenUserPropertyName);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User is not authenticated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogWarning_WhenUserIdClaimIsMissing()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser@example.com"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Items.Should().NotContainKey(AuthConfig.JWTTokenUserPropertyName);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to parse User ID from claims")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogWarning_WhenUserIdClaimIsNotInteger()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-number"),
            new Claim(ClaimTypes.Name, "testuser@example.com")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Items.Should().NotContainKey(AuthConfig.JWTTokenUserPropertyName);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to parse User ID from claims")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCreateClaimsIdentity_WhenUserNotAuthenticatedButHasClaims()
    {
        // Arrange
        var userId = 456;
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser@example.com")
        };
        var identity = new ClaimsIdentity(claims); // Not authenticated (no authenticationType)
        context.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.User.Identity.Should().NotBeNull();
        context.User.Identity!.IsAuthenticated.Should().BeTrue();
        context.User.Identity.AuthenticationType.Should().Be("Bearer");
        context.Items.Should().ContainKey(AuthConfig.JWTTokenUserPropertyName);
        context.Items[AuthConfig.JWTTokenUserPropertyName].Should().Be(userId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAlwaysCallNext()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999)]
    [InlineData(int.MaxValue)]
    public async Task InvokeAsync_ShouldHandleVariousUserIds(int userId)
    {
        // Arrange
        var context = CreateHttpContextWithAuthenticatedUser(userId);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Items[AuthConfig.JWTTokenUserPropertyName].Should().Be(userId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotThrowException_WhenContextUserIsNull()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            User = null!
        };

        // Act
        Func<Task> act = async () => await _middleware.InvokeAsync(context);

        // Assert
        await act.Should().NotThrowAsync();
        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldStoreUserIdInHttpContextItems()
    {
        // Arrange
        var userId = 789;
        var context = CreateHttpContextWithAuthenticatedUser(userId);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey(AuthConfig.JWTTokenUserPropertyName);
        var storedUserId = context.Items[AuthConfig.JWTTokenUserPropertyName];
        storedUserId.Should().BeOfType<int>();
        storedUserId.Should().Be(userId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleEmptyClaimsList()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(); // Empty claims
        context.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Items.Should().NotContainKey(AuthConfig.JWTTokenUserPropertyName);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogInformation_WhenUserIdExtractedSuccessfully()
    {
        // Arrange
        var userId = 555;
        var context = CreateHttpContextWithAuthenticatedUser(userId);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Extracted User ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotOverwriteExistingAuthentication()
    {
        // Arrange
        var userId = 111;
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser@example.com")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        var originalUser = context.User;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.User.Should().Be(originalUser);
        context.User.Identity!.IsAuthenticated.Should().BeTrue();
        context.Items[AuthConfig.JWTTokenUserPropertyName].Should().Be(userId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleMultipleClaims()
    {
        // Arrange
        var userId = 222;
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser@example.com"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Items[AuthConfig.JWTTokenUserPropertyName].Should().Be(userId);
        context.User.Claims.Should().HaveCount(5);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("12.34")]
    [InlineData("null")]
    public async Task InvokeAsync_ShouldHandleInvalidUserIdFormats(string invalidUserId)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, invalidUserId)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        
        // int.TryParse will successfully parse these values, so they will be stored
        if (int.TryParse(invalidUserId, out var parsedId))
        {
            context.Items.Should().ContainKey(AuthConfig.JWTTokenUserPropertyName);
            context.Items[AuthConfig.JWTTokenUserPropertyName].Should().Be(parsedId);
        }
        else
        {
            context.Items.Should().NotContainKey(AuthConfig.JWTTokenUserPropertyName);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to parse User ID from claims")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task InvokeAsync_ShouldExtractUserId_EvenForZeroAndNegativeValues(int userId)
    {
        // Arrange
        var context = CreateHttpContextWithAuthenticatedUser(userId);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Items.Should().ContainKey(AuthConfig.JWTTokenUserPropertyName);
        context.Items[AuthConfig.JWTTokenUserPropertyName].Should().Be(userId);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Extracted User ID: {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static HttpContext CreateHttpContextWithAuthenticatedUser(int userId)
    {
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser@example.com")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }
}
