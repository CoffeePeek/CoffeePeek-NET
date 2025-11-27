using CoffeePeek.BuildingBlocks.AuthOptions;
using FluentAssertions;

namespace CoffeePeek.BuildingBlocks.Tests.AuthOptions;

public class JWTOptionsTests
{
    [Fact]
    public void JWTOptions_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var options = new JWTOptions();

        // Assert
        options.Should().NotBeNull();
        options.SecretKey.Should().BeNull();
        options.Issuer.Should().BeNull();
        options.Audience.Should().BeNull();
        options.AccessTokenLifetimeMinutes.Should().Be(0);
        options.RefreshTokenLifetimeDays.Should().Be(0);
    }

    [Fact]
    public void JWTOptions_Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new JWTOptions();

        // Act
        options.SecretKey = "test-secret-key";
        options.Issuer = "test-issuer";
        options.Audience = "test-audience";
        options.AccessTokenLifetimeMinutes = 15;
        options.RefreshTokenLifetimeDays = 7;

        // Assert
        options.SecretKey.Should().Be("test-secret-key");
        options.Issuer.Should().Be("test-issuer");
        options.Audience.Should().Be("test-audience");
        options.AccessTokenLifetimeMinutes.Should().Be(15);
        options.RefreshTokenLifetimeDays.Should().Be(7);
    }

    [Theory]
    [InlineData(5, 1)]
    [InlineData(15, 7)]
    [InlineData(30, 30)]
    [InlineData(60, 90)]
    public void JWTOptions_ShouldHandleVariousLifetimeValues(int accessMinutes, int refreshDays)
    {
        // Arrange & Act
        var options = new JWTOptions
        {
            AccessTokenLifetimeMinutes = accessMinutes,
            RefreshTokenLifetimeDays = refreshDays
        };

        // Assert
        options.AccessTokenLifetimeMinutes.Should().Be(accessMinutes);
        options.RefreshTokenLifetimeDays.Should().Be(refreshDays);
    }

    [Fact]
    public void JWTOptions_ShouldAllowEmptyStrings()
    {
        // Arrange & Act
        var options = new JWTOptions
        {
            SecretKey = "",
            Issuer = "",
            Audience = ""
        };

        // Assert
        options.SecretKey.Should().BeEmpty();
        options.Issuer.Should().BeEmpty();
        options.Audience.Should().BeEmpty();
    }
}

public class AuthenticationOptionsTests
{
    [Fact]
    public void AuthenticationOptions_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var options = new AuthenticationOptions();

        // Assert
        options.Should().NotBeNull();
        options.JwtSecretKey.Should().BeNull();
        options.ValidIssuer.Should().BeNull();
        options.ValidAudience.Should().BeNull();
        options.ExpireIntervalMinutes.Should().Be(0);
        options.ExpireRefreshIntervalDays.Should().Be(0);
    }

    [Fact]
    public void AuthenticationOptions_Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new AuthenticationOptions
        {
            // Act
            JwtSecretKey = "my-super-secret-key",
            ValidIssuer = "my-issuer",
            ValidAudience = "my-audience",
            ExpireIntervalMinutes = 30,
            ExpireRefreshIntervalDays = 14,
        };

        // Assert
        options.JwtSecretKey.Should().Be("my-super-secret-key");
        options.ValidIssuer.Should().Be("my-issuer");
        options.ValidAudience.Should().Be("my-audience");
        options.ExpireIntervalMinutes.Should().Be(30);
        options.ExpireRefreshIntervalDays.Should().Be(14);
    }

    [Theory]
    [InlineData(10, 5)]
    [InlineData(20, 15)]
    [InlineData(45, 60)]
    [InlineData(120, 180)]
    public void AuthenticationOptions_ShouldHandleVariousExpirationValues(int expireMinutes, int refreshDays)
    {
        // Arrange & Act
        var options = new AuthenticationOptions
        {
            ExpireIntervalMinutes = expireMinutes,
            ExpireRefreshIntervalDays = refreshDays
        };

        // Assert
        options.ExpireIntervalMinutes.Should().Be(expireMinutes);
        options.ExpireRefreshIntervalDays.Should().Be(refreshDays);
    }

    [Fact]
    public void AuthenticationOptions_ShouldAllowLongSecretKeys()
    {
        // Arrange
        var longKey = new string('a', 256);

        // Act
        var options = new AuthenticationOptions
        {
            JwtSecretKey = longKey
        };

        // Assert
        options.JwtSecretKey.Should().HaveLength(256);
        options.JwtSecretKey.Should().Be(longKey);
    }
}
