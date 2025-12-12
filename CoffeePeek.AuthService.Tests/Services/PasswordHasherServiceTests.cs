using CoffeePeek.AuthService.Services;
using FluentAssertions;
using Xunit;

namespace CoffeePeek.AuthService.Tests.Services;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _sut = new();

    [Fact]
    public void HashPassword_WithValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _sut.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_WithSamePassword_ReturnsDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "each hash should use a unique salt");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("VeryLongPasswordThatExceedsNormalLimitsButShouldStillBeHashedCorrectly123456789012345678901234567890")]
    public void HashPassword_WithVariousLengths_ReturnsValidHash(string password)
    {
        // Act
        var hash = _sut.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void HashPassword_WithEmptyOrWhitespace_ThrowsArgumentException(string password)
    {
        // Act & Assert
        var act = () => _sut.HashPassword(password);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(hash, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var correctPassword = "TestPassword123!";
        var incorrectPassword = "WrongPassword456!";
        var hash = _sut.HashPassword(correctPassword);

        // Act
        var result = _sut.VerifyPassword(hash, incorrectPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _sut.HashPassword(password);

        // Act & Assert
        var act = () => _sut.VerifyPassword(hash, "");
        act.Should().Throw<ArgumentException>()
            .WithParameterName("providedPassword");
    }

    [Fact]
    public void VerifyPassword_WithNullHash_ThrowsArgumentException()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act & Assert
        var act = () => _sut.VerifyPassword(null!, password);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("hashedPassword");
    }

    [Fact]
    public void VerifyPassword_WithInvalidHashFormat_ThrowsFormatException()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "InvalidHashFormat";

        // Act & Assert
        var act = () => _sut.VerifyPassword(invalidHash, password);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void VerifyPassword_WithCaseSensitivePassword_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(hash, "testpassword123!");

        // Assert
        result.Should().BeFalse("passwords should be case-sensitive");
    }

    [Fact]
    public void HashPassword_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var password = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act
        var hash = _sut.HashPassword(password);
        var isValid = _sut.VerifyPassword(hash, password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        isValid.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_WithUnicodeCharacters_HandlesCorrectly()
    {
        // Arrange
        var password = "Пароль123!☕🔒";

        // Act
        var hash = _sut.HashPassword(password);
        var isValid = _sut.VerifyPassword(hash, password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        isValid.Should().BeTrue();
    }
}