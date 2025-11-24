using CoffeePeek.BusinessLogic.Abstractions;
using FluentAssertions;

namespace CoffeePeek.BusinessLogic.Tests.Abstractions;

public class ValidationResultTests
{
    [Fact]
    public void ValidationResult_Valid_ShouldReturnValidResult()
    {
        // Act
        var result = ValidationResult.Valid;

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ValidationResult_Invalid_ShouldReturnInvalidResult()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var result = ValidationResult.Invalid(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void ValidationResult_WithNullErrorMessage_ShouldBeValid()
    {
        // Arrange
        var result = new ValidationResult { ErrorMessage = null };

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Error message")]
    [InlineData("Another error message with more details")]
    public void ValidationResult_WithVariousErrorMessages_ShouldMaintainValidityState(string errorMessage)
    {
        // Act
        var result = ValidationResult.Invalid(errorMessage);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
    }
}