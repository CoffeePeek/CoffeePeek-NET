using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Services.Validation;
using FluentAssertions;
using Xunit;

namespace CoffeePeek.AuthService.Tests.Validation;

public class UserCreateValidationStrategyTests
{
    private readonly UserCreateValidationStrategy _sut;

    public UserCreateValidationStrategyTests()
    {
        _sut = new UserCreateValidationStrategy();
    }

    [Theory]
    [InlineData("test@example.com", "ValidPass123", true)]
    [InlineData("user.name@domain.co.uk", "SecureP@ss", true)]
    [InlineData("user+tag@example.com", "MyPass99", true)]
    [InlineData("user_name@test-domain.com", "Pass1234", true)]
    public void Validate_WithValidEmailAndPassword_ReturnsValid(string email, string password, bool expected)
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", email, password);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("short")]
    [InlineData("a")]
    [InlineData("")]
    public void Validate_WithPasswordTooShort_ReturnsInvalid(string password)
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "test@example.com", password);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("between 6 and 30 characters");
    }

    [Fact]
    public void Validate_WithPasswordTooLong_ReturnsInvalid()
    {
        // Arrange
        var password = new string('a', 31);
        var command = new RegisterUserCommand("TestUser", "test@example.com", password);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("between 6 and 30 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    [InlineData("user..name@example.com")]
    [InlineData("user@domain")]
    [InlineData("user name@example.com")]
    public void Validate_WithInvalidEmail_ReturnsInvalid(string email)
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", email, "ValidPass123");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email address");
    }

    [Fact]
    public void Validate_WithMinimumValidPassword_ReturnsValid()
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "test@example.com", "123456");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMaximumValidPassword_ReturnsValid()
    {
        // Arrange
        var password = new string('a', 30);
        var command = new RegisterUserCommand("TestUser", "test@example.com", password);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("test@EXAMPLE.COM")]
    [InlineData("TEST@example.com")]
    [InlineData("TeSt@ExAmPlE.cOm")]
    public void Validate_WithMixedCaseEmail_ReturnsValid(string email)
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", email, "ValidPass123");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("user@sub.domain.example.com")]
    [InlineData("user@a.b.c.d.com")]
    public void Validate_WithSubdomainsInEmail_ReturnsValid(string email)
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", email, "ValidPass123");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithSpecialCharactersInPassword_ReturnsValid()
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "test@example.com", "P@ss!w0rd#");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}