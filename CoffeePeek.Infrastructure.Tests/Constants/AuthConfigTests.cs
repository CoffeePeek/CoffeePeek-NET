using CoffeePeek.Infrastructure.Constants;
using FluentAssertions;

namespace CoffeePeek.Infrastructure.Tests.Constants;

public class AuthConfigTests
{
    [Fact]
    public void JWTTokenUserPropertyName_ShouldBeUserId()
    {
        // Assert
        AuthConfig.JWTTokenUserPropertyName.Should().Be("UserId");
    }

    [Fact]
    public void JWTTokenUserPropertyName_ShouldBeConstant()
    {
        // Arrange & Act
        var value1 = AuthConfig.JWTTokenUserPropertyName;
        var value2 = AuthConfig.JWTTokenUserPropertyName;

        // Assert
        value1.Should().Be(value2);
        value1.Should().BeOfType<string>();
    }

    [Fact]
    public void JWTTokenUserPropertyName_ShouldNotBeEmpty()
    {
        // Assert
        AuthConfig.JWTTokenUserPropertyName.Should().NotBeNullOrEmpty();
    }
}
