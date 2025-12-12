using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response.User;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.UserService.Handlers;
using CoffeePeek.UserService.Models;
using CoffeePeek.UserService.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.UserService.Tests.Handlers;

public class UpdateProfileHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRedisService> _redisServiceMock = new();
    private readonly UpdateProfileHandler _sut;

    public UpdateProfileHandlerTests()
    {
        _sut = new UpdateProfileHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _redisServiceMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsError()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            UserId = Guid.NewGuid(),
            UserName = "new",
            Email = "new@example.com"
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(request.UserId)).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("User not found");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesUserAndCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "old",
            Email = "old@example.com",
            About = "old about"
        };

        var request = new UpdateProfileRequest
        {
            UserId = userId,
            UserName = " new-name ",
            Email = " new@example.com ",
            About = "updated about"
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeOfType<UpdateProfileResponse>();

        user.Username.Should().Be("new-name");
        user.Email.Should().Be("new@example.com");
        user.About.Should().Be("updated about");

        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _redisServiceMock.Verify(r => r.RemoveAsync(CacheKey.User.Profile(userId)), Times.Once);
        _redisServiceMock.Verify(r => r.RemoveAsync(CacheKey.User.ById(userId)), Times.Once);
        _redisServiceMock.Verify(r => r.RemoveAsync(CacheKey.User.ByEmail("old@example.com")), Times.Once);

        _redisServiceMock.Verify(r => r.SetAsync(CacheKey.User.ById(userId), user, null), Times.Once);
        _redisServiceMock.Verify(r => r.SetAsync(CacheKey.User.ByEmail("new@example.com"), user, null), Times.Once);
    }
}

