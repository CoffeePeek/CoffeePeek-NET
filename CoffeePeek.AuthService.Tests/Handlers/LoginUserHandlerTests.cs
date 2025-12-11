using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Handlers;
using CoffeePeek.AuthService.Models;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoffeePeek.AuthService.Tests.Handlers;

public class LoginUserHandlerTests
{
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly Mock<IUserManager> _userManagerMock;
    private readonly Mock<IJWTTokenService> _jwtTokenServiceMock;
    private readonly Mock<ISignInManager> _signInManagerMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly LoginUserHandler _sut;

    public LoginUserHandlerTests()
    {
        _redisServiceMock = new Mock<IRedisService>();
        _userManagerMock = new Mock<IUserManager>();
        _jwtTokenServiceMock = new Mock<IJWTTokenService>();
        _signInManagerMock = new Mock<ISignInManager>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger>();

        _sut = new LoginUserHandler(
            _redisServiceMock.Object,
            _userManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _signInManagerMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCredentialsFromCache_ReturnsSuccessWithTokens()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "ValidPassword123");
        var user = new UserCredentials
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashedpassword"
        };
        var signInResult = new SignInResultWrapper { Result = SignInResult.Success };
        var authResult = new AuthResult
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token"
        };

        _redisServiceMock
            .Setup(x => x.GetAsync<UserCredentials>(It.IsAny<string>()))
            .ReturnsAsync(user);
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, command.Password))
            .ReturnsAsync(signInResult);
        _jwtTokenServiceMock
            .Setup(x => x.GenerateTokensAsync(user))
            .ReturnsAsync(authResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access_token");
        result.Data.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task Handle_WithValidCredentialsFromDatabase_ReturnsSuccessWithTokens()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "ValidPassword123");
        var user = new UserCredentials
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashedpassword"
        };
        var signInResult = new SignInResultWrapper { Result = SignInResult.Success };
        var authResult = new AuthResult
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token"
        };

        _redisServiceMock
            .Setup(x => x.GetAsync<UserCredentials>(It.IsAny<string>()))
            .ReturnsAsync((UserCredentials?)null);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, command.Password))
            .ReturnsAsync(signInResult);
        _jwtTokenServiceMock
            .Setup(x => x.GenerateTokensAsync(user))
            .ReturnsAsync(authResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsError()
    {
        // Arrange
        var command = new LoginUserCommand("nonexistent@example.com", "password");

        _redisServiceMock
            .Setup(x => x.GetAsync<UserCredentials>(It.IsAny<string>()))
            .ReturnsAsync((UserCredentials?)null);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((UserCredentials?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Account does not exist");
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ReturnsError()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "WrongPassword");
        var user = new UserCredentials
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashedpassword"
        };
        var signInResult = new SignInResultWrapper { Result = SignInResult.Failed };

        _redisServiceMock
            .Setup(x => x.GetAsync<UserCredentials>(It.IsAny<string>()))
            .ReturnsAsync(user);
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, command.Password))
            .ReturnsAsync(signInResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Password is incorrect");
    }

    [Fact]
    public async Task Handle_WhenUserManagerThrowsException_ReturnsError()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "password");
        var exceptionMessage = "Database connection failed";

        _redisServiceMock
            .Setup(x => x.GetAsync<UserCredentials>(It.IsAny<string>()))
            .ReturnsAsync((UserCredentials?)null);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task Handle_WithEmptyEmail_ReturnsError()
    {
        // Arrange
        var command = new LoginUserCommand("", "password");

        _redisServiceMock
            .Setup(x => x.GetAsync<UserCredentials>(It.IsAny<string>()))
            .ReturnsAsync((UserCredentials?)null);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((UserCredentials?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithSuccessfulLogin_PublishesUserLoggedInEvent()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "ValidPassword123");
        var user = new UserCredentials
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashedpassword"
        };
        var signInResult = new SignInResultWrapper { Result = SignInResult.Success };
        var authResult = new AuthResult
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token"
        };

        _redisServiceMock
            .Setup(x => x.GetAsync<UserCredentials>(It.IsAny<string>()))
            .ReturnsAsync(user);
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, command.Password))
            .ReturnsAsync(signInResult);
        _jwtTokenServiceMock
            .Setup(x => x.GenerateTokensAsync(user))
            .ReturnsAsync(authResult);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}