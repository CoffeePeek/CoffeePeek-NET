using CoffeePeek.BusinessLogic.RequestHandlers.Login;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Login;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Infrastructure.Auth;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.Login;

public class LoginRequestHandlerTests
{
    private readonly Mock<IJWTTokenService> _jwtTokenServiceMock;
    private readonly Mock<UserManager<CoffeePeek.Domain.Entities.Users.User>> _userManagerMock;
    private readonly Mock<SignInManager<CoffeePeek.Domain.Entities.Users.User>> _signInManagerMock;
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly LoginRequestHandler _handler;

    public LoginRequestHandlerTests()
    {
        // Setup JWTTokenService mock
        _jwtTokenServiceMock = new Mock<IJWTTokenService>();
        
        // Setup UserManager mock
        var userStoreMock = new Mock<IUserStore<CoffeePeek.Domain.Entities.Users.User>>();
        _userManagerMock = new Mock<UserManager<CoffeePeek.Domain.Entities.Users.User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
            
        // Setup SignInManager mock
        _signInManagerMock = new Mock<SignInManager<CoffeePeek.Domain.Entities.Users.User>>(
            _userManagerMock.Object,
            Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<CoffeePeek.Domain.Entities.Users.User>>(),
            null!, null!, null!, null!);
            
        // Setup RedisService mock
        _redisServiceMock = new Mock<IRedisService>();
        
        // Create handler instance
        _handler = new LoginRequestHandler(
            _jwtTokenServiceMock.Object,
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _redisServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUserNotFoundInCacheOrDatabase()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        CoffeePeek.Domain.Entities.Users.User? userFromCache = null;
        CoffeePeek.Domain.Entities.Users.User? userFromDb = null;
        
        _redisServiceMock
            .Setup(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(request.Email))
            .ReturnsAsync(userFromCache);
            
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(userFromDb);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Account does not exist.");

        _redisServiceMock.Verify(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(request.Email), Times.Once);
        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _signInManagerMock.Verify(s => s.CheckPasswordSignInAsync(It.IsAny<CoffeePeek.Domain.Entities.Users.User>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenPasswordIsIncorrect()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "wrongpassword");
        var user = new CoffeePeek.Domain.Entities.Users.User { Email = "test@example.com", UserName = "Test User" };
        CoffeePeek.Domain.Entities.Users.User? userFromCache = null;
        var signInResult = SignInResult.Failed;
        
        _redisServiceMock
            .Setup(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(request.Email))
            .ReturnsAsync(userFromCache);
            
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
            
        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(signInResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Password is incorrect.");

        _redisServiceMock.Verify(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(request.Email), Times.Once);
        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _signInManagerMock.Verify(s => s.CheckPasswordSignInAsync(user, request.Password, true), Times.Once);
        _jwtTokenServiceMock.Verify(j => j.GenerateTokensAsync(It.IsAny<CoffeePeek.Domain.Entities.Users.User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenLoginIsSuccessful()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "correctpassword");
        var user = new CoffeePeek.Domain.Entities.Users.User { Id = 1, Email = "test@example.com", UserName = "Test User" };
        CoffeePeek.Domain.Entities.Users.User? userFromCache = null;
        var signInResult = SignInResult.Success;
        var authResult = new AuthResult
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
        };

        _redisServiceMock
            .Setup(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(request.Email))
            .ReturnsAsync(userFromCache);
            
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
            
        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(signInResult);
            
        _jwtTokenServiceMock
            .Setup(j => j.GenerateTokensAsync(user))
            .ReturnsAsync(authResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access_token");
        result.Data.RefreshToken.Should().Be("refresh_token");

        _redisServiceMock.Verify(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(request.Email), Times.Once);
        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _signInManagerMock.Verify(s => s.CheckPasswordSignInAsync(user, request.Password, true), Times.Once);
        _jwtTokenServiceMock.Verify(j => j.GenerateTokensAsync(user), Times.Once);
        _redisServiceMock.Verify(r => r.SetAsync($"{nameof(CoffeePeek.Domain.Entities.Users.User)}{user.Id}", user), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserFromCache_WhenUserExistsInCache()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "correctpassword");
        var user = new CoffeePeek.Domain.Entities.Users.User { Id = 1, Email = "test@example.com", UserName = "Test User" };
        var signInResult = SignInResult.Success;
        var authResult = new AuthResult
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
        };
        
        _redisServiceMock
            .Setup(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(request.Email))
            .ReturnsAsync(user);
            
        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(signInResult);
            
        _jwtTokenServiceMock
            .Setup(j => j.GenerateTokensAsync(user))
            .ReturnsAsync(authResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        _redisServiceMock.Verify(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(request.Email), Times.Once);
        _userManagerMock.Verify(u => u.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        _signInManagerMock.Verify(s => s.CheckPasswordSignInAsync(user, request.Password, true), Times.Once);
        _jwtTokenServiceMock.Verify(j => j.GenerateTokensAsync(user), Times.Once);
    }
}