using CoffeePeek.Account.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CoffeePeek.Account.Domain.Tests;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string token = "refresh_token_123";
        var ttl = TimeSpan.FromDays(7);
        const string device = "Chrome/Windows";
        const string ip = "192.168.1.1";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var refreshToken = new RefreshToken(userId, token, ttl, device, ip);
        var afterCreation = DateTime.UtcNow;

        // Assert
        refreshToken.UserId.Should().Be(userId);
        refreshToken.Token.Should().Be(token);
        refreshToken.Device.Should().Be(device);
        refreshToken.IpAddress.Should().Be(ip);
        refreshToken.IsActive.Should().BeTrue();
        refreshToken.CreatedDate.Should().BeAfter(beforeCreation).And.BeBefore(afterCreation.AddSeconds(1));
        refreshToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(ttl), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_ShouldDeactivateToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = new RefreshToken(userId, "token", TimeSpan.FromDays(7), "device", "ip");

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsActive.Should().BeFalse();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_CalledTwice_ShouldOnlyRevokeOnce()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = new RefreshToken(userId, "token", TimeSpan.FromDays(7), "device", "ip");
        var firstRevokeTime = DateTime.UtcNow;

        // Act
        refreshToken.Revoke();
        Thread.Sleep(100);
        refreshToken.Revoke();

        // Assert
        refreshToken.IsActive.Should().BeFalse();
        refreshToken.RevokedAt.Should().BeCloseTo(firstRevokeTime, TimeSpan.FromMilliseconds(200));
    }

    [Fact]
    public void IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = new RefreshToken(userId, "token", TimeSpan.FromDays(7), "device", "ip");

        // Act
        var isExpired = refreshToken.ExpiresAt < DateTime.UtcNow;

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpired_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = new RefreshToken(userId, "token", TimeSpan.FromMilliseconds(-1), "device", "ip");

        // Act
        Thread.Sleep(10); // Ensure time has passed
        var isExpired = refreshToken.ExpiresAt < DateTime.UtcNow;

        // Assert
        isExpired.Should().BeTrue();
    }

    [Theory]
    [InlineData("Chrome/Windows")]
    [InlineData("Firefox/Linux")]
    [InlineData("Safari/MacOS")]
    [InlineData("Mobile App/iOS")]
    public void Constructor_WithDifferentDevices_ShouldStoreDevice(string device)
    {
        // Arrange & Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token", TimeSpan.FromDays(7), device, "ip");

        // Assert
        refreshToken.Device.Should().Be(device);
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    public void Constructor_WithDifferentIPs_ShouldStoreIP(string ip)
    {
        // Arrange & Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token", TimeSpan.FromDays(7), "device", ip);

        // Assert
        refreshToken.IpAddress.Should().Be(ip);
    }

    [Theory]
    [InlineData(1)] // 1 day
    [InlineData(7)] // 1 week
    [InlineData(30)] // 1 month
    [InlineData(90)] // 3 months
    public void Constructor_WithDifferentTTLs_ShouldSetCorrectExpiration(int days)
    {
        // Arrange
        var ttl = TimeSpan.FromDays(days);
        var beforeCreation = DateTime.UtcNow;

        // Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token", ttl, "device", "ip");

        // Assert
        var expectedExpiration = beforeCreation.Add(ttl);
        refreshToken.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void IsActive_AfterRevoke_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token", TimeSpan.FromDays(7), "device", "ip");

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CreatedDate_ShouldBeSetToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token", TimeSpan.FromDays(7), "device", "ip");

        // Assert
        refreshToken.CreatedDate.Kind.Should().Be(DateTimeKind.Utc);
        refreshToken.CreatedDate.Should().BeCloseTo(beforeCreation, TimeSpan.FromSeconds(1));
    }
}