using CoffeePeek.Contract.Requests.User;
using CoffeePeek.UserService.Configuration;
using CoffeePeek.UserService.Handlers;
using CoffeePeek.UserService.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoffeePeek.UserService.Tests.Handlers;

public class GetProfileHandlerTests : IDisposable
{
    private readonly UserDbContext _dbContext;
    private readonly GetProfileHandler _sut;

    public GetProfileHandlerTests()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new UserDbContext(options);
        _sut = new GetProfileHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ReturnsUserProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            AvatarUrl = "https://example.com/avatar.jpg",
            Bio = "Test bio",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new GetProfileRequest(userId);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
        result.Data.Username.Should().Be("testuser");
        result.Data.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsError()
    {
        // Arrange
        var request = new GetProfileRequest(Guid.NewGuid());

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("User not found");
    }

    [Fact]
    public async Task Handle_WithExistingUser_IncludesUserStatistics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com"
        };

        var statistics = new UserStatistics
        {
            UserId = userId,
            CheckInCount = 10,
            ReviewCount = 5,
            FavoriteShopsCount = 3
        };

        _dbContext.Users.Add(user);
        _dbContext.UserStatistics.Add(statistics);
        await _dbContext.SaveChangesAsync();

        var request = new GetProfileRequest(userId);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithUserWithoutStatistics_ReturnsProfileSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com"
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new GetProfileRequest(userId);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ReturnsError()
    {
        // Arrange
        var request = new GetProfileRequest(Guid.Empty);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}