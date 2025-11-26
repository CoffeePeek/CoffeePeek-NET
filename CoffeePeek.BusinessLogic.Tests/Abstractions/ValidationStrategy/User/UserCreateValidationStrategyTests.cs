using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.Contract.Dtos.User;
using FluentAssertions;

namespace CoffeePeek.BusinessLogic.Tests.Abstractions.ValidationStrategy.User;

public class UserCreateValidationStrategyTests
{
    private readonly UserCreateValidationStrategy _validator = new();

    [Fact]
    public void UserCreateValidationStrategy_Validate_WithValidUser_ShouldReturnValidResult()
    {
        // Arrange
        var userDto = new UserDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        // Act
        var result = _validator.Validate(userDto);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("short", "Password must be between 6 and 30 characters")] // Too short
    [InlineData("this_password_is_way_too_long_and_exceeds_the_maximum_length_allowed",
        "Password must be between 6 and 30 characters")] // Too long
    public void UserCreateValidationStrategy_Validate_WithInvalidPassword_ShouldReturnInvalidResult(string password,
        string expectedError)
    {
        // Arrange
        var userDto = new UserDto
        {
            Email = "test@example.com",
            Password = password
        };

        // Act
        var result = _validator.Validate(userDto);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Theory]
    [InlineData("invalid-email", "Invalid email address")] // Missing @
    [InlineData("@example.com", "Invalid email address")] // Missing local part
    [InlineData("test@", "Invalid email address")] // Missing domain
    [InlineData("test@.com", "Invalid email address")] // Invalid domain
    [InlineData("test@example.", "Invalid email address")] // Missing TLD
    public void UserCreateValidationStrategy_Validate_WithInvalidEmail_ShouldReturnInvalidResult(string email,
        string expectedError)
    {
        // Arrange
        var userDto = new UserDto
        {
            Email = email,
            Password = "password123"
        };

        // Act
        var result = _validator.Validate(userDto);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Fact]
    public void UserCreateValidationStrategy_Validate_WithValidEmailFormats_ShouldReturnValidResult()
    {
        // Arrange
        var validEmails = new[]
        {
            "test@example.com",
            "user.name@domain.comuk",
            "user+tag@example.org",
            "user123@test-domain.com"
        };

        foreach (var email in validEmails)
        {
            var userDto = new UserDto
            {
                Email = email,
                Password = "password123"
            };

            // Act
            var result = _validator.Validate(userDto);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue($"Email '{email}' should be valid");
            result.ErrorMessage.Should().BeNull();
        }
    }

    [Theory]
    [InlineData("valid@example.com", "validpass")] // Minimum valid password
    [InlineData("valid@example.com", "this_is_a_very_long_but")] // Maximum valid password
    public void UserCreateValidationStrategy_Validate_WithBoundaryPasswordLengths_ShouldReturnValidResult(string email,
        string password)
    {
        // Arrange
        var userDto = new UserDto
        {
            Email = email,
            Password = password
        };

        // Act
        var result = _validator.Validate(userDto);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }
}