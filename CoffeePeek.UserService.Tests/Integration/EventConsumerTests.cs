using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.UserService.Configuration;
using CoffeePeek.UserService.Models;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoffeePeek.UserService.Tests.Integration;

public class EventConsumerTests(UserServiceWebApplicationFactory factory) : IClassFixture<UserServiceWebApplicationFactory>
{
    private async Task ClearDatabaseAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();
        
        // Clear database
        dbContext.UserStatistics.RemoveRange(dbContext.UserStatistics);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync();
        
        // Clear Redis (if needed, but Redis is mocked in tests)
    }

    [Fact]
    public async Task UserRegisteredEventConsumer_CreatesUserAndCachesIt()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var userId = Guid.NewGuid();
        var @event = new UserRegisteredEvent(
            UserId: userId,
            Email: "test@example.com",
            UserName: "testuser"
        );

        using var scope = factory.Services.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();

        // Act
        await publishEndpoint.Publish(@event);
        
        // Wait for consumer to process
        await Task.Delay(500);

        // Assert
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        user.Should().NotBeNull();
        user!.Email.Should().Be("test@example.com");
        user.Username.Should().Be("testuser");
        
        // Verify cache (if Redis is not mocked, this might fail)
        var cachedUser = await redisService.GetAsync<Models.User>(CacheKey.User.ById(userId));
        cachedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task CoffeeShopApprovedEventConsumer_CreatesStatistics()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var userId = Guid.NewGuid();
        
        // Create User first (required for foreign key)
        var user = new Models.User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser"
        };
        
        using var setupScope = factory.Services.CreateScope();
        var setupDbContext = setupScope.ServiceProvider.GetRequiredService<UserDbContext>();
        setupDbContext.Users.Add(user);
        await setupDbContext.SaveChangesAsync();
        
        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Shop",
            NotValidatedAddress: "Test Address",
            UserId: userId,
            address: "Test Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: Array.Empty<string>(),
            Schedules: Array.Empty<CoffeePeek.Contract.Dtos.Schedule.ScheduleDto>(),
            Latitude: null,
            Longitude: null
        );

        using var scope = factory.Services.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Act
        await publishEndpoint.Publish(@event);
        
        // Wait for consumer to process (give it more time)
        await Task.Delay(2000);

        // Assert - use new scope to get fresh data
        using var assertScope = factory.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<UserDbContext>();
        var statistics = await assertDbContext.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);
        statistics.Should().NotBeNull();
        statistics!.AddedShopsCount.Should().Be(1);
        statistics.CheckInCount.Should().Be(0);
        statistics.ReviewCount.Should().Be(0);
    }

    [Fact]
    public async Task CoffeeShopApprovedEventConsumer_IncrementsExistingStatistics()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var userId = Guid.NewGuid();
        
        // Create User first (required for foreign key)
        var user = new Models.User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser"
        };
        
        var existingStatistics = new UserStatistics
        {
            UserId = userId,
            CheckInCount = 5,
            ReviewCount = 3,
            AddedShopsCount = 2,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        dbContext.Users.Add(user);
        dbContext.UserStatistics.Add(existingStatistics);
        await dbContext.SaveChangesAsync();

        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Shop",
            NotValidatedAddress: "Test Address",
            UserId: userId,
            address: "Test Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: Array.Empty<string>(),
            Schedules: Array.Empty<CoffeePeek.Contract.Dtos.Schedule.ScheduleDto>(),
            Latitude: null,
            Longitude: null
        );

        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Act
        await publishEndpoint.Publish(@event);
        
        // Wait for consumer to process (give it more time)
        await Task.Delay(2000);

        // Assert - use new scope to get fresh data
        using var assertScope = factory.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<UserDbContext>();
        var statistics = await assertDbContext.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);
        statistics.Should().NotBeNull();
        statistics!.AddedShopsCount.Should().Be(3);
        statistics.CheckInCount.Should().Be(5);
        statistics.ReviewCount.Should().Be(3);
    }

    [Fact]
    public async Task ReviewAddedEventConsumer_CreatesStatistics()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var userId = Guid.NewGuid();
        
        // Create User first (required for foreign key)
        var user = new Models.User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser"
        };
        
        using var setupScope = factory.Services.CreateScope();
        var setupDbContext = setupScope.ServiceProvider.GetRequiredService<UserDbContext>();
        setupDbContext.Users.Add(user);
        await setupDbContext.SaveChangesAsync();
        
        var @event = new ReviewAddedEvent
        {
            UserId = userId,
            ShopId = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        using var scope = factory.Services.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Act
        await publishEndpoint.Publish(@event);
        
        // Wait for consumer to process (give it more time)
        await Task.Delay(2000);

        // Assert - use new scope to get fresh data
        using var assertScope = factory.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<UserDbContext>();
        var statistics = await assertDbContext.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);
        statistics.Should().NotBeNull();
        statistics!.ReviewCount.Should().Be(1);
        statistics.CheckInCount.Should().Be(0);
        statistics.AddedShopsCount.Should().Be(0);
    }

    [Fact]
    public async Task ReviewAddedEventConsumer_IncrementsExistingStatistics()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var userId = Guid.NewGuid();
        
        // Create User first (required for foreign key)
        var user = new Models.User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser"
        };
        
        var existingStatistics = new UserStatistics
        {
            UserId = userId,
            CheckInCount = 5,
            ReviewCount = 3,
            AddedShopsCount = 2,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        dbContext.Users.Add(user);
        dbContext.UserStatistics.Add(existingStatistics);
        await dbContext.SaveChangesAsync();

        var @event = new ReviewAddedEvent
        {
            UserId = userId,
            ShopId = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Act
        await publishEndpoint.Publish(@event);
        
        // Wait for consumer to process (give it more time)
        await Task.Delay(2000);

        // Assert - use new scope to get fresh data
        using var assertScope = factory.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<UserDbContext>();
        var statistics = await assertDbContext.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);
        statistics.Should().NotBeNull();
        statistics!.ReviewCount.Should().Be(4);
        statistics.CheckInCount.Should().Be(5);
        statistics.AddedShopsCount.Should().Be(2);
    }

    [Fact]
    public async Task CheckinCreatedEventConsumer_CreatesStatistics()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var userId = Guid.NewGuid();
        
        // Create User first (required for foreign key)
        var user = new Models.User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser"
        };
        
        using var setupScope = factory.Services.CreateScope();
        var setupDbContext = setupScope.ServiceProvider.GetRequiredService<UserDbContext>();
        setupDbContext.Users.Add(user);
        await setupDbContext.SaveChangesAsync();
        
        var @event = new CheckinCreatedEvent
        {
            UserId = userId,
            ShopId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        using var scope = factory.Services.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Act
        await publishEndpoint.Publish(@event);
        
        // Wait for consumer to process (give it more time)
        await Task.Delay(2000);

        // Assert - use new scope to get fresh data
        using var assertScope = factory.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<UserDbContext>();
        var statistics = await assertDbContext.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);
        statistics.Should().NotBeNull();
        statistics!.CheckInCount.Should().Be(1);
        statistics.ReviewCount.Should().Be(0);
        statistics.AddedShopsCount.Should().Be(0);
    }

    [Fact]
    public async Task CheckinCreatedEventConsumer_IncrementsExistingStatistics()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var userId = Guid.NewGuid();
        
        // Create User first (required for foreign key)
        var user = new Models.User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser"
        };
        
        var existingStatistics = new UserStatistics
        {
            UserId = userId,
            CheckInCount = 5,
            ReviewCount = 3,
            AddedShopsCount = 2,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        dbContext.Users.Add(user);
        dbContext.UserStatistics.Add(existingStatistics);
        await dbContext.SaveChangesAsync();

        var @event = new CheckinCreatedEvent
        {
            UserId = userId,
            ShopId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Act
        await publishEndpoint.Publish(@event);
        
        // Wait for consumer to process (give it more time)
        await Task.Delay(2000);

        // Assert - use new scope to get fresh data
        using var assertScope = factory.Services.CreateScope();
        var assertDbContext = assertScope.ServiceProvider.GetRequiredService<UserDbContext>();
        var statistics = await assertDbContext.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);
        statistics.Should().NotBeNull();
        statistics!.CheckInCount.Should().Be(6);
        statistics.ReviewCount.Should().Be(3);
        statistics.AddedShopsCount.Should().Be(2);
    }
}

