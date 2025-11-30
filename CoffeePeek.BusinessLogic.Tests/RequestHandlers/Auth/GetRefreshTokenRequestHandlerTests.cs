using CoffeePeek.BusinessLogic.RequestHandlers;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Infrastructure.Auth;
using FluentAssertions;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.Auth;

public class GetRefreshTokenRequestHandlerTests
{
    private readonly Mock<IJWTTokenService> _jwtTokenServiceMock;
    private readonly GetRefreshTokenRequestHandler _handler;

    public GetRefreshTokenRequestHandlerTests()
    {
        // Setup JWTTokenService mock
        _jwtTokenServiceMock = new Mock<IJWTTokenService>();
        
        // Create handler instance
        _handler = new GetRefreshTokenRequestHandler(_jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRefreshTokenIsValid()
    {
        // Arrange
        var request = new GetRefreshTokenRequest("valid_refresh_token")
        {
            UserId = 1
        };
        var authResult = new AuthResult
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
        };
        var expectedResponse = new GetRefreshTokenResponse("new_access_token", "new_refresh_token");

        _jwtTokenServiceMock
            .Setup(j => j.RefreshTokensAsync(request.RefreshToken, request.UserId))
            .ReturnsAsync(authResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("new_access_token");
        result.Data.RefreshToken.Should().Be("new_refresh_token");

        _jwtTokenServiceMock.Verify(j => j.RefreshTokensAsync(request.RefreshToken, request.UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var request = new GetRefreshTokenRequest("invalid_refresh_token")
        {
            UserId = 1
        };
        var exception = new UnauthorizedAccessException("Invalid refresh token");

        _jwtTokenServiceMock
            .Setup(j => j.RefreshTokensAsync(request.RefreshToken, request.UserId))
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid refresh token");

        _jwtTokenServiceMock.Verify(j => j.RefreshTokensAsync(request.RefreshToken, request.UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var request = new GetRefreshTokenRequest("refresh_token")
        {
            UserId = 1  
        };
        var exception = new Exception("Unexpected error");

        _jwtTokenServiceMock
            .Setup(j => j.RefreshTokensAsync(request.RefreshToken, request.UserId))
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error occurred");

        _jwtTokenServiceMock.Verify(j => j.RefreshTokensAsync(request.RefreshToken, request.UserId), Times.Once);
    }
}