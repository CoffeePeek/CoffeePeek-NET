using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Handlers;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.AuthService.Tests.Handlers;

public class RefreshTokenHandlerTests
{
    private readonly Mock<IJWTTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RefreshTokenHandler _sut;

    public RefreshTokenHandlerTests()
    {
        _sut = new RefreshTokenHandler(_jwtTokenServiceMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RefreshTokenCommand("valid-refresh-token")
        {
            UserId = userId
        };

        var authResult = new AuthResult
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiredAt = DateTime.UtcNow.AddMinutes(30)
        };

        _jwtTokenServiceMock
            .Setup(x => x.RefreshTokensAsync(request.RefreshToken, userId))
            .ReturnsAsync(authResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be(authResult.AccessToken);
        result.Data.RefreshToken.Should().Be(authResult.RefreshToken);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ReturnsError()
    {
        // Arrange
        var request = new RefreshTokenCommand("invalid-token")
        {
            UserId = Guid.NewGuid()
        };

        _jwtTokenServiceMock
            .Setup(x => x.RefreshTokensAsync(request.RefreshToken, request.UserId))
            .ThrowsAsync(new UnauthorizedAccessException("invalid"));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Invalid refresh token");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnsGenericError()
    {
        // Arrange
        var request = new RefreshTokenCommand("any-token")
        {
            UserId = Guid.NewGuid()
        };

        _jwtTokenServiceMock
            .Setup(x => x.RefreshTokensAsync(request.RefreshToken, request.UserId))
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Error occurred");
        result.Data.Should().BeNull();
    }
}

