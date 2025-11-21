using System.IdentityModel.Tokens.Jwt;
using CoffeePeek.BuildingBlocks.AuthOptions;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.Repositories.Interfaces;
using CoffeePeek.Infrastructure.Auth;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Moq;

namespace CoffeePeek.Infrastructure.Tests.Auth;

[TestSubject(typeof(JWTTokenService))]
public class JWTTokenServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IOptions<JWTOptions>> _optionsMock;
    private readonly JWTTokenService _service;
    private readonly JWTOptions _testOptions;

    public JWTTokenServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _optionsMock = new Mock<IOptions<JWTOptions>>();

        _testOptions = new JWTOptions
        {
            SecretKey = "super_secret_key_that_is_at_least_32_chars_long_to_be_secure_12345",
            AccessTokenLifetimeMinutes = 15,
            RefreshTokenLifetimeDays = 7,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };
        _optionsMock.Setup(o => o.Value).Returns(_testOptions);

        _service = new JWTTokenService(_optionsMock.Object, _userRepositoryMock.Object);
    }

[Fact]
    public async Task GenerateTokensAsync_ShouldReturnValidAuthResultAndSaveRefreshToken()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, Email = "test@example.com" };

        // Act
        var result = await _service.GenerateTokensAsync(user);

        // Assert
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        
        Assert.True(result.ExpiredAt > DateTime.UtcNow);
        Assert.True(result.ExpiredAt < DateTime.UtcNow.AddMinutes(_testOptions.AccessTokenLifetimeMinutes + 1));
        
        var newRefreshToken = user.RefreshTokens.LastOrDefault();
        Assert.NotNull(newRefreshToken);
        Assert.Equal(result.RefreshToken, newRefreshToken.Token);
        Assert.Equal(userId, newRefreshToken.UserId);
        
        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.AccessToken);
        Assert.Equal(userId.ToString(), jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Email, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldGenerateNewTokens_WhenValidTokenIsProvided()
    {
        // Arrange
        var userId = 1;
        var existingRefreshTokenString = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Email = "refresh@example.com",
            RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken { Token = existingRefreshTokenString, UserId = userId, ExpiryDate = DateTime.UtcNow.AddDays(7) }
            }
        };

        // Настройка мока: при запросе по старому токену возвращаем пользователя
        _userRepositoryMock
            .Setup(r => r.GetUserByRefreshToken(existingRefreshTokenString))
            .ReturnsAsync(user);

        // Act
        var result = await _service.RefreshTokensAsync(existingRefreshTokenString);

        // Assert
        // 1. Проверка, что Refresh Token был отозван
        var oldToken = user.RefreshTokens.FirstOrDefault(t => t.Token == existingRefreshTokenString);
        Assert.NotNull(oldToken);
        Assert.True(oldToken.IsRevoked);

        // 2. Проверка, что создан новый Refresh Token
        var newToken = user.RefreshTokens.Last();
        Assert.NotEqual(existingRefreshTokenString, newToken.Token);
        Assert.Equal(result.RefreshToken, newToken.Token);

        // 3. Проверка, что репозиторий был вызван для сохранения (дважды: Update и SaveChangesAsync внутри GenerateTokensAsync)
        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // 4. Проверка, что Access Token новый
        Assert.NotNull(result.AccessToken);
    }
    
    [Fact]
    public async Task RefreshTokensAsync_ShouldThrowUnauthorizedException_WhenUserNotFound()
    {
        // Arrange
        var invalidToken = Guid.NewGuid().ToString();
        
        // Настройка мока: не находим пользователя
        _userRepositoryMock
            .Setup(r => r.GetUserByRefreshToken(invalidToken))
            .ReturnsAsync((User)null!); 

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.RefreshTokensAsync(invalidToken)
        );
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldThrowUnauthorizedException_WhenTokenIsExpired()
    {
        // Arrange
        var userId = 1;
        var expiredTokenString = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Email = "expired@example.com",
            RefreshTokens = new List<RefreshToken>
            {
                // Устанавливаем дату истечения в прошлом
                new RefreshToken 
                { 
                    Token = expiredTokenString, 
                    UserId = userId, 
                    ExpiryDate = DateTime.UtcNow.AddDays(-1) 
                }
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetUserByRefreshToken(expiredTokenString))
            .ReturnsAsync(user);

        // Act & Assert
        // Код проверяет token.IsActive, который будет false
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.RefreshTokensAsync(expiredTokenString)
        );
    }

    [Fact]
    public async Task RefreshTokensAsync_ShouldThrowUnauthorizedException_WhenTokenIsRevoked()
    {
        // Arrange
        var userId = 1;
        var revokedTokenString = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Email = "revoked@example.com",
            RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken 
                { 
                    Token = revokedTokenString, 
                    UserId = userId, 
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsRevoked = true // Токен отозван
                }
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetUserByRefreshToken(revokedTokenString))
            .ReturnsAsync(user);

        // Act & Assert
        // Код проверяет token.IsActive, который будет false
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.RefreshTokensAsync(revokedTokenString)
        );
    }
}