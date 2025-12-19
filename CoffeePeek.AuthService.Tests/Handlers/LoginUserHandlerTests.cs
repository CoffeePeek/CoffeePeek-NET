using CoffeePeek.Auth.Application.Commands;
using CoffeePeek.Auth.Application.Handlers;
using CoffeePeek.Auth.Application.Services;
using CoffeePeek.Auth.Domain.Entities;
using CoffeePeek.AuthService.Models;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoffeePeek.AuthService.Tests.Handlers;

public class LoginUserHandlerTests
{
    private readonly Mock<IHybridCache> _cacheMock;
    private readonly Mock<IUserManager> _userManagerMock;
    private readonly Mock<IJWTTokenService> _jwtTokenServiceMock;
    private readonly Mock<ISignInManager> _signInManagerMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LoginUserHandler _sut;

    public LoginUserHandlerTests()
    {
        _cacheMock = new Mock<IHybridCache>();
        _userManagerMock = new Mock<IUserManager>();
        _jwtTokenServiceMock = new Mock<IJWTTokenService>();
        _signInManagerMock = new Mock<ISignInManager>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<LoginUserHandler>>();

        _sut = new LoginUserHandler(
            _cacheMock.Object,
            _userManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _signInManagerMock.Object,
            _publishEndpointMock.Object,
            _unitOfWorkMock.Object,
            loggerMock.Object
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
        var signInResult = SignInResultWrapper.Success;
        var authResult = new AuthResult
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token"
        };

        _cacheMock
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<CoffeePeek.Shared.Infrastructure.Cache.CacheKey>(),
                It.IsAny<Func<Task<UserCredentials?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cacheMock
            .Setup(x => x.SetAsync(
                It.IsAny<CoffeePeek.Shared.Infrastructure.Cache.CacheKey>(),
                It.IsAny<UserCredentials>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, command.Password))
            .ReturnsAsync(signInResult);
        _jwtTokenServiceMock
            .Setup(x => x.GenerateTokensAsync(user, command.DeviceName, command.IpAddress))
            .ReturnsAsync(authResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access_token");
        result.Data.RefreshToken.Should().Be("refresh_token");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        var signInResult = SignInResultWrapper.Success;
        var authResult = new AuthResult
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token"
        };

        _cacheMock
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<Task<UserCredentials?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns((CacheKey _, Func<Task<UserCredentials?>> factory, TimeSpan? _, TimeSpan? __, CancellationToken ___)
                => factory());
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, command.Password))
            .ReturnsAsync(signInResult);
        _jwtTokenServiceMock
            .Setup(x => x.GenerateTokensAsync(user, command.DeviceName, command.IpAddress))
            .ReturnsAsync(authResult);
        _cacheMock
            .Setup(x => x.SetAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<UserCredentials>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsError()
    {
        // Arrange
        var command = new LoginUserCommand("nonexistent@example.com", "password");

        _cacheMock
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<CoffeePeek.Shared.Infrastructure.Cache.CacheKey>(),
                It.IsAny<Func<Task<UserCredentials?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns((CoffeePeek.Shared.Infrastructure.Cache.CacheKey _, Func<Task<UserCredentials?>> factory, TimeSpan? _, TimeSpan? __, CancellationToken ___)
                => factory());
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((UserCredentials?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Account does not exist");
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
        var signInResult = SignInResultWrapper.Failed;

        _cacheMock
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<CoffeePeek.Shared.Infrastructure.Cache.CacheKey>(),
                It.IsAny<Func<Task<UserCredentials?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, command.Password))
            .ReturnsAsync(signInResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Password is incorrect");
    }

    [Fact]
    public async Task Handle_WhenUserManagerThrowsException_ReturnsError()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "password");
        var exceptionMessage = "Database connection failed";

        _cacheMock
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<CoffeePeek.Shared.Infrastructure.Cache.CacheKey>(),
                It.IsAny<Func<Task<UserCredentials?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task Handle_WithEmptyEmail_ReturnsError()
    {
        // Arrange
        var command = new LoginUserCommand("", "password");

        _cacheMock
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<Shared.Infrastructure.Cache.CacheKey>(),
                It.IsAny<Func<Task<UserCredentials?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
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
        var signInResult = SignInResultWrapper.Success;
        var authResult = new AuthResult
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token"
        };

        var publishCalled = new TaskCompletionSource<bool>();
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<UserLoggedInEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => publishCalled.SetResult(true));

        _cacheMock
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<CoffeePeek.Shared.Infrastructure.Cache.CacheKey>(),
                It.IsAny<Func<Task<UserCredentials?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, command.Password))
            .ReturnsAsync(signInResult);
        _jwtTokenServiceMock
            .Setup(x => x.GenerateTokensAsync(user, command.DeviceName, command.IpAddress))
            .ReturnsAsync(authResult);
        _cacheMock
            .Setup(x => x.SetAsync(
                It.IsAny<CoffeePeek.Shared.Infrastructure.Cache.CacheKey>(),
                It.IsAny<UserCredentials>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Wait for the background Task.Run to complete (with timeout)
        await Task.WhenAny(publishCalled.Task, Task.Delay(TimeSpan.FromSeconds(2)));

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<UserLoggedInEvent>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}