using CoffeePeek.Infrastructure.Services.Secure;
using FluentAssertions;

namespace CoffeePeek.Infrastructure.Tests.Secure;

public class HashingServiceTests
{
    private readonly HashingService _hashingService;

    public HashingServiceTests()
    {
        _hashingService = new HashingService();
    }

    [Fact]
    public void HashString_ShouldReturnHashedString_WhenValidPasswordProvided()
    {
        // Arrange
        var password = "MySecretPassword123!";

        // Act
        var hashedPassword = _hashingService.HashString(password);

        // Assert
        hashedPassword.Should().NotBeNullOrWhiteSpace();
        hashedPassword.Should().NotBe(password);
        hashedPassword.Length.Should().BeGreaterThan(20); // Base64 encoded hash should be reasonably long
    }

    [Fact]
    public void HashString_ShouldThrowArgumentException_WhenPasswordIsNull()
    {
        // Arrange
        string? password = null;

        // Act
        Action act = () => _hashingService.HashString(password!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be null or empty.*");
    }

    [Fact]
    public void HashString_ShouldThrowArgumentException_WhenPasswordIsEmpty()
    {
        // Arrange
        var password = "";

        // Act
        Action act = () => _hashingService.HashString(password);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be null or empty.*");
    }

    [Fact]
    public void HashString_ShouldThrowArgumentException_WhenPasswordIsWhitespace()
    {
        // Arrange
        var password = "   ";

        // Act
        Action act = () => _hashingService.HashString(password);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password cannot be null or empty.*");
    }

    [Fact]
    public void VerifyHashedStrings_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        // Arrange
        var password = "MySecretPassword123!";
        var hashedPassword = _hashingService.HashString(password);

        // Act
        var result = _hashingService.VerifyHashedStrings(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyHashedStrings_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        var password = "MySecretPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hashedPassword = _hashingService.HashString(password);

        // Act
        var result = _hashingService.VerifyHashedStrings(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHashedStrings_ShouldReturnFalse_WhenPasswordIsNull()
    {
        // Arrange
        string? password = null;
        var hashedPassword = "someHashedPassword";

        // Act
        var result = _hashingService.VerifyHashedStrings(password!, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHashedStrings_ShouldReturnFalse_WhenHashedPasswordIsNull()
    {
        // Arrange
        var password = "MySecretPassword123!";
        string? hashedPassword = null;

        // Act
        var result = _hashingService.VerifyHashedStrings(password, hashedPassword!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHashedStrings_ShouldReturnFalse_WhenPasswordIsEmpty()
    {
        // Arrange
        var password = "";
        var hashedPassword = "someHashedPassword";

        // Act
        var result = _hashingService.VerifyHashedStrings(password, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHashedStrings_ShouldReturnFalse_WhenHashedPasswordIsEmpty()
    {
        // Arrange
        var password = "MySecretPassword123!";
        var hashedPassword = "";

        // Act
        var result = _hashingService.VerifyHashedStrings(password, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHashedStrings_ShouldReturnFalse_WhenHashedPasswordIsInvalidFormat()
    {
        // Arrange
        var password = "MySecretPassword123!";
        var invalidHashedPassword = "invalidFormat"; // Not a valid Base64 string for our hash

        // Act
        var result = _hashingService.VerifyHashedStrings(password, invalidHashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateRandomPassword_ShouldReturnPasswordWithDefaultLength_WhenNoLengthSpecified()
    {
        // Arrange & Act
        var password = _hashingService.GenerateRandomPassword();

        // Assert
        password.Should().NotBeNullOrWhiteSpace();
        password.Should().HaveLength(12); // Default length is 12
    }

    [Fact]
    public void GenerateRandomPassword_ShouldReturnPasswordWithSpecifiedLength_WhenLengthProvided()
    {
        // Arrange
        const int length = 16;

        // Act
        var password = _hashingService.GenerateRandomPassword(length);

        // Assert
        password.Should().NotBeNullOrWhiteSpace();
        password.Should().HaveLength(length);
    }

    [Fact]
    public void GenerateRandomPassword_ShouldThrowArgumentException_WhenLengthIsLessThan8()
    {
        // Arrange
        const int length = 5;

        // Act
        Action act = () => _hashingService.GenerateRandomPassword(length);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password length should be at least 8 characters.*");
    }

    [Fact]
    public void GenerateRandomPassword_ShouldGenerateDifferentPasswords_EachTime()
    {
        // Arrange
        var passwords = new HashSet<string>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var password = _hashingService.GenerateRandomPassword();
            passwords.Add(password);
        }

        // Assert
        passwords.Should().HaveCount(10); // All passwords should be unique
    }

    [Fact]
    public void HashString_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        // Arrange
        var password = "MySecretPassword123!";

        // Act
        var hash1 = _hashingService.HashString(password);
        var hash2 = _hashingService.HashString(password);

        // Assert
        hash1.Should().NotBeNullOrWhiteSpace();
        hash2.Should().NotBeNullOrWhiteSpace();
        hash1.Should().NotBe(hash2); // Different salts should produce different hashes
    }
}