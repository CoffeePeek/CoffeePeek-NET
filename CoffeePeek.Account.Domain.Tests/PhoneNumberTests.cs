using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Extensions.Exceptions;
using FluentAssertions;
using Xunit;

namespace CoffeePeek.Account.Domain.Tests;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+12345678901234")]
    [InlineData("+79991234567")]
    [InlineData("+441234567890")]
    [InlineData("1234567890")] // Should add + automatically
    [InlineData("+375 29 123 45 67")] // Spaces should be removed
    [InlineData("+375-29-123-45-67")] // Dashes should be removed
    [InlineData("+375(29)1234567")] // Parentheses should be removed
    public void Create_WithValidPhoneNumber_ShouldSucceed(string validPhone)
    {
        // Act
        var phone = PhoneNumber.Create(validPhone);

        // Assert
        phone.Should().NotBeNull();
        phone.Value.Should().StartWith("+");
        phone.Value.Should().MatchRegex(@"^\+[1-9]\d{1,14}$");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldThrowDomainException(string? invalidPhone)
    {
        // Act
        Action act = () => PhoneNumber.Create(invalidPhone!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Phone number*empty*");
    }

    [Theory]
    [InlineData("+123")] // Too short (less than 4 digits after +)
    [InlineData("+123456789012345")] // Too long (more than 14 digits after +)
    [InlineData("abc123456789")] // Contains letters
    [InlineData("+12a3456789")] // Contains letter after +
    [InlineData("+0123456789")] // Starts with 0 after +
    public void Create_WithInvalidFormat_ShouldThrowDomainException(string invalidPhone)
    {
        // Act
        Action act = () => PhoneNumber.Create(invalidPhone);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*phone number format*");
    }

    [Fact]
    public void Create_WithSpaces_ShouldRemoveSpaces()
    {
        // Arrange
        const string phoneWithSpaces = "+375 29 123 45 67";

        // Act
        var phone = PhoneNumber.Create(phoneWithSpaces);

        // Assert
        phone.Value.Should().NotContain(" ");
        phone.Value.Should().Be("+37529123456 7");
    }

    [Fact]
    public void Create_WithDashes_ShouldRemoveDashes()
    {
        // Arrange
        const string phoneWithDashes = "+375-29-123-45-67";

        // Act
        var phone = PhoneNumber.Create(phoneWithDashes);

        // Assert
        phone.Value.Should().NotContain("-");
    }

    [Fact]
    public void Create_WithParentheses_ShouldRemoveParentheses()
    {
        // Arrange
        const string phoneWithParentheses = "+375(29)1234567";

        // Act
        var phone = PhoneNumber.Create(phoneWithParentheses);

        // Assert
        phone.Value.Should().NotContain("(");
        phone.Value.Should().NotContain(")");
    }

    [Fact]
    public void Create_WithoutPlusPrefix_ShouldAddPlus()
    {
        // Arrange
        const string phoneWithoutPlus = "1234567890";

        // Act
        var phone = PhoneNumber.Create(phoneWithoutPlus);

        // Assert
        phone.Value.Should().StartWith("+");
        phone.Value.Should().Be("+1234567890");
    }

    [Theory]
    [InlineData("+37529", "A1")]
    [InlineData("+375292345678", "A1")]
    [InlineData("+375295234567", "A1")]
    [InlineData("+375297234567", "A1")]
    [InlineData("+375298234567", "A1")]
    [InlineData("+375291234567", "MTS")]
    [InlineData("+375293234567", "MTS")]
    [InlineData("+375294234567", "MTS")]
    [InlineData("+375296234567", "MTS")]
    [InlineData("+375299234567", "MTS")]
    [InlineData("+375441234567", "A1")]
    [InlineData("+375331234567", "MTS")]
    [InlineData("+375251234567", "Life")]
    [InlineData("+375171234567", "Landline (Minsk)")]
    [InlineData("+375161234567", "Other Belarusian")]
    [InlineData("+11234567890", "International / Unknown")]
    [InlineData("+441234567890", "International / Unknown")]
    public void GetBelarusianOperator_ShouldReturnCorrectOperator(string phoneNumber, string expectedOperator)
    {
        // Arrange
        var phone = PhoneNumber.Create(phoneNumber);

        // Act
        var operator = phone.GetBelarusianOperator();

        // Assert
        operator.Should().Be(expectedOperator);
    }

    [Fact]
    public void ImplicitConversion_FromPhoneNumber_ShouldReturnValue()
    {
        // Arrange
        var phone = PhoneNumber.Create("+1234567890");

        // Act
        string value = phone;

        // Assert
        value.Should().Be("+1234567890");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var phone = PhoneNumber.Create("+1234567890");

        // Act
        var result = phone.ToString();

        // Assert
        result.Should().Be("+1234567890");
    }

    [Fact]
    public void Equals_WithSamePhoneNumber_ShouldReturnTrue()
    {
        // Arrange
        var phone1 = PhoneNumber.Create("+1234567890");
        var phone2 = PhoneNumber.Create("+1234567890");

        // Act & Assert
        phone1.Equals(phone2).Should().BeTrue();
        (phone1 == phone2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentPhoneNumber_ShouldReturnFalse()
    {
        // Arrange
        var phone1 = PhoneNumber.Create("+1234567890");
        var phone2 = PhoneNumber.Create("+9876543210");

        // Act & Assert
        phone1.Equals(phone2).Should().BeFalse();
        (phone1 != phone2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSamePhoneNumber_ShouldReturnSameHash()
    {
        // Arrange
        var phone1 = PhoneNumber.Create("+1234567890");
        var phone2 = PhoneNumber.Create("+1234567890");

        // Act & Assert
        phone1.GetHashCode().Should().Be(phone2.GetHashCode());
    }
}