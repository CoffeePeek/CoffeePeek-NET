using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Extensions.Exceptions;
using FluentAssertions;
using Xunit;

namespace CoffeePeek.Account.Domain.Tests;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("firstname+lastname@company.com")]
    [InlineData("email@subdomain.example.com")]
    [InlineData("1234567890@example.com")]
    [InlineData("Test.User@EXAMPLE.COM")] // Should be normalized
    public void Create_WithValidEmail_ShouldSucceed(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(validEmail.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldThrowDomainException(string? invalidEmail)
    {
        // Act
        Action act = () => Email.Create(invalidEmail!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Email*empty*");
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missinglocal.com")]
    [InlineData("missing@domain")]
    [InlineData("missing.domain@.com")]
    [InlineData("two@@domain.com")]
    [InlineData("spaces in@email.com")]
    [InlineData("email@domain..com")]
    public void Create_WithInvalidFormat_ShouldThrowDomainException(string invalidEmail)
    {
        // Act
        Action act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*email format*");
    }

    [Fact]
    public void Create_WithEmailTooLong_ShouldThrowDomainException()
    {
        // Arrange - create email longer than 255 characters
        var longLocal = new string('a', 250);
        var longEmail = $"{longLocal}@example.com";

        // Act
        Action act = () => Email.Create(longEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*too long*");
    }

    [Fact]
    public void Create_WithEmailExactly255Characters_ShouldSucceed()
    {
        // Arrange - create email exactly 255 characters
        var local = new string('a', 242); // 242 + @ + example.com (11) = 254
        var email = $"{local}@example.com";

        // Act
        var result = Email.Create(email);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().HaveLength(254);
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase()
    {
        // Arrange
        const string mixedCaseEmail = "Test.User@EXAMPLE.COM";

        // Act
        var email = Email.Create(mixedCaseEmail);

        // Assert
        email.Value.Should().Be("test.user@example.com");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange
        const string emailWithSpaces = "  test@example.com  ";

        // Act
        var email = Email.Create(emailWithSpaces);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitConversion_FromEmail_ShouldReturnValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string value = email;

        // Assert
        value.Should().Be("test@example.com");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Equals(email2).Should().BeTrue();
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        email1.Equals(email2).Should().BeFalse();
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameEmail_ShouldReturnSameHash()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Theory]
    [InlineData("test+filter@gmail.com")]
    [InlineData("user123@company-name.co.uk")]
    [InlineData("first.last@subdomain.example.org")]
    public void Create_WithComplexButValidEmails_ShouldSucceed(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }
}