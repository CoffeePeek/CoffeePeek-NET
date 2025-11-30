using CoffeePeek.BuildingBlocks.AuthOptions;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Infrastructure.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace CoffeePeek.Infrastructure.Tests.Auth;

public class JWTTokenServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IOptions<JWTOptions>> _optionsMock;
    private readonly JWTTokenService _jwtTokenService;
    private readonly JWTOptions _jwtOptions;

    public JWTTokenServiceTests()
    {
        // Setup UserManager mock
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        // Setup JWTOptions
        _jwtOptions = new JWTOptions
        {
            SecretKey = "ThisIsASecretKeyThatIsAtLeast32CharactersLong12345",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenLifetimeMinutes = 30
        };
        
        _optionsMock = new Mock<IOptions<JWTOptions>>();
        _optionsMock.Setup(o => o.Value).Returns(_jwtOptions);

        _jwtTokenService = new JWTTokenService(_userManagerMock.Object, _optionsMock.Object);
    }

    [Fact]
    public async Task GenerateTokensAsync_ShouldReturnAuthResult_WhenUserIsValid()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@example.com"
        };
        
        var refreshToken = "generated-refresh-token";
        var roles = new List<string> { "user" };

        _userManagerMock
            .Setup(um => um.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken"))
            .ReturnsAsync(refreshToken);

        _userManagerMock
            .Setup(um => um.SetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken", refreshToken))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _jwtTokenService.GenerateTokensAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().Be(refreshToken);

        _userManagerMock.Verify(
            um => um.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken"),
            Times.Once);

        _userManagerMock.Verify(
            um => um.SetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken", refreshToken),
            Times.Once);

        _userManagerMock.Verify(
            um => um.GetRolesAsync(user),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var userId = 1;
        var refreshToken = "valid-refresh-token";
        
        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com"
        };
        
        var newRefreshToken = "new-refresh-token";
        var roles = new List<string> { "user" };

        _userManagerMock
            .Setup(um => um.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(um => um.GetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken"))
            .ReturnsAsync(refreshToken);

        _userManagerMock
            .Setup(um => um.RemoveAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(um => um.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken"))
            .ReturnsAsync(newRefreshToken);

        _userManagerMock
            .Setup(um => um.SetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken", newRefreshToken))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _jwtTokenService.RefreshTokensAsync(refreshToken, userId);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().Be(newRefreshToken);

        _userManagerMock.Verify(
            um => um.FindByIdAsync(userId.ToString()),
            Times.Once);

        _userManagerMock.Verify(
            um => um.GetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken"),
            Times.Once);

        _userManagerMock.Verify(
            um => um.RemoveAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken"),
            Times.Once);

        _userManagerMock.Verify(
            um => um.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken"),
            Times.Once);

        _userManagerMock.Verify(
            um => um.SetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken", newRefreshToken),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
    {
        // Arrange
        var userId = 999;
        var refreshToken = "any-refresh-token";

        _userManagerMock
            .Setup(um => um.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _jwtTokenService.RefreshTokensAsync(refreshToken, userId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid user.");

        _userManagerMock.Verify(
            um => um.FindByIdAsync(userId.ToString()),
            Times.Once);

        _userManagerMock.Verify(
            um => um.GetAuthenticationTokenAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldThrowUnauthorizedAccessException_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var userId = 1;
        var providedRefreshToken = "provided-refresh-token";
        var storedRefreshToken = "different-stored-refresh-token";

        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com"
        };

        _userManagerMock
            .Setup(um => um.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(um => um.GetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken"))
            .ReturnsAsync(storedRefreshToken);

        // Act
        Func<Task> act = async () => await _jwtTokenService.RefreshTokensAsync(providedRefreshToken, userId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid refresh token.");

        _userManagerMock.Verify(
            um => um.FindByIdAsync(userId.ToString()),
            Times.Once);

        _userManagerMock.Verify(
            um => um.GetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken"),
            Times.Once);

        _userManagerMock.Verify(
            um => um.RemoveAuthenticationTokenAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}