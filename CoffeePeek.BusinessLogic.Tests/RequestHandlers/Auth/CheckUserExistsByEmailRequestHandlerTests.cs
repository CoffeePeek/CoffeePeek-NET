using CoffeePeek.BusinessLogic.RequestHandlers;
using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.Auth;

public class CheckUserExistsByEmailRequestHandlerTests
{
    private readonly Mock<UserManager<CoffeePeek.Domain.Entities.Users.User>> _userManagerMock;
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly CheckUserExistsByEmailRequestHandler _handler;

    public CheckUserExistsByEmailRequestHandlerTests()
    {
        // Setup UserManager mock
        var userStoreMock = new Mock<IUserStore<CoffeePeek.Domain.Entities.Users.User>>();
        _userManagerMock = new Mock<UserManager<CoffeePeek.Domain.Entities.Users.User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        // Setup RedisService mock
        _redisServiceMock = new Mock<IRedisService>();

        // Create handler instance
        _handler = new CheckUserExistsByEmailRequestHandler(
            _userManagerMock.Object,
            _redisServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserExistsInCache()
    {
        // Arrange
        var email = "test@example.com";
        var request = new CheckUserExistsByEmailRequest(email);
        var userFromCache = new CoffeePeek.Domain.Entities.Users.User { Email = email, UserName = "Test User" };

        _redisServiceMock
            .Setup(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(It.IsAny<string>()))
            .ReturnsAsync(userFromCache);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        _redisServiceMock.Verify(
            r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(It.Is<string>(key => key.Contains(email))),
            Times.Once);

        _userManagerMock.Verify(
            u => u.FindByEmailAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessAndCacheUser_WhenUserExistsInDatabase()
    {
        // Arrange
        var email = "test@example.com";
        var request = new CheckUserExistsByEmailRequest(email);
        CoffeePeek.Domain.Entities.Users.User? userFromCache = null;
        var userFromDb = new CoffeePeek.Domain.Entities.Users.User { Email = email, UserName = "Test User" };

        _redisServiceMock
            .Setup(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(It.IsAny<string>()))
            .ReturnsAsync(userFromCache);

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(userFromDb);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        _redisServiceMock.Verify(
            r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(It.Is<string>(key => key.Contains(email))),
            Times.Once);

        _userManagerMock.Verify(
            u => u.FindByEmailAsync(email),
            Times.Once);

        _redisServiceMock.Verify(
            r => r.SetAsync(
                It.Is<string>(key => key.Contains(email)),
                userFromDb,
                It.Is<TimeSpan>(ts => ts == TimeSpan.FromMinutes(5))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var request = new CheckUserExistsByEmailRequest(email);
        CoffeePeek.Domain.Entities.Users.User? userFromCache = null;
        CoffeePeek.Domain.Entities.Users.User? userFromDb = null;

        _redisServiceMock
            .Setup(r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(It.IsAny<string>()))
            .ReturnsAsync(userFromCache);

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(userFromDb);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");

        _redisServiceMock.Verify(
            r => r.GetAsync<CoffeePeek.Domain.Entities.Users.User>(It.Is<string>(key => key.Contains(email))),
            Times.Once);

        _userManagerMock.Verify(
            u => u.FindByEmailAsync(email),
            Times.Once);

        _redisServiceMock.Verify(
            r => r.SetAsync(It.IsAny<string>(), It.IsAny<CoffeePeek.Domain.Entities.Users.User>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }
}