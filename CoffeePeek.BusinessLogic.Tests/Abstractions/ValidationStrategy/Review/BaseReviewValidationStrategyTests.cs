using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.BusinessLogic.Abstractions.Review;
using FluentAssertions;

namespace CoffeePeek.BusinessLogic.Tests.Abstractions.ValidationStrategy.Review;

// Test class to access protected members of BaseReviewValidationStrategy
public class TestBaseReviewValidationStrategy : BaseReviewValidationStrategy
{
    public static ValidationResult PublicValidateUserId(int userId) => ValidateUserId(userId);
    public static ValidationResult PublicValidateHeader(string header) => ValidateHeader(header);
    public static ValidationResult PublicValidateComment(string comment) => ValidateComment(comment);
    public static bool PublicIsValidRating(int rating) => IsValidRating(rating);
}

public class BaseReviewValidationStrategyTests
{
    [Fact]
    public void BaseReviewValidationStrategy_ValidateUserId_WithValidUserId_ShouldReturnValidResult()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateUserId(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData(0, "UserId must be greater than 0")]
    [InlineData(-1, "UserId must be greater than 0")]
    [InlineData(-100, "UserId must be greater than 0")]
    public void BaseReviewValidationStrategy_ValidateUserId_WithInvalidUserId_ShouldReturnInvalidResult(int userId,
        string expectedError)
    {
        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateUserId(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Fact]
    public void BaseReviewValidationStrategy_ValidateHeader_WithValidHeader_ShouldReturnValidResult()
    {
        // Arrange
        var header = "Valid header text";

        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateHeader(header);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "Header is required")]
    [InlineData("", "Header is required")]
    [InlineData("  ", "Header is required")]
    public void BaseReviewValidationStrategy_ValidateHeader_WithNullOrWhiteSpaceHeader_ShouldReturnInvalidResult(
        string header, string expectedError)
    {
        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateHeader(header);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Theory]
    [InlineData("A", "Header must be between 3 and 100 characters")] // Too short
    [InlineData("AB", "Header must be between 3 and 100 characters")] // Too short
    public void BaseReviewValidationStrategy_ValidateHeader_WithTooShortHeader_ShouldReturnInvalidResult(string header,
        string expectedError)
    {
        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateHeader(header);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Fact]
    public void BaseReviewValidationStrategy_ValidateHeader_WithExactlyMaxHeaderLength_ShouldReturnValidResult()
    {
        // Arrange
        var header = new string('A', 100); // Exactly 100 characters

        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateHeader(header);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void BaseReviewValidationStrategy_ValidateHeader_WithTooLongHeader_ShouldReturnInvalidResult()
    {
        // Arrange
        var header = new string('A', 101); // 101 characters, exceeds max of 100
        var expectedError = "Header must be between 3 and 100 characters";

        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateHeader(header);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Fact]
    public void BaseReviewValidationStrategy_ValidateComment_WithValidComment_ShouldReturnValidResult()
    {
        // Arrange
        var comment = "This is a valid comment with sufficient length to pass validation.";

        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateComment(comment);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "Comment is required")]
    [InlineData("", "Comment is required")]
    [InlineData("  ", "Comment is required")]
    public void BaseReviewValidationStrategy_ValidateComment_WithNullOrWhiteSpaceComment_ShouldReturnInvalidResult(
        string comment, string expectedError)
    {
        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateComment(comment);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Fact]
    public void BaseReviewValidationStrategy_ValidateComment_WithTooShortComment_ShouldReturnInvalidResult()
    {
        // Arrange
        var comment = "Too short"; // 9 characters, less than minimum of 10
        var expectedError = "Comment must be between 10 and 1000 characters";

        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateComment(comment);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Fact]
    public void BaseReviewValidationStrategy_ValidateComment_WithExactlyMinCommentLength_ShouldReturnValidResult()
    {
        // Arrange
        var comment = "1234567890"; // Exactly 10 characters

        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateComment(comment);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void BaseReviewValidationStrategy_ValidateComment_WithExactlyMaxCommentLength_ShouldReturnValidResult()
    {
        // Arrange
        var comment = new string('A', 1000); // Exactly 1000 characters

        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateComment(comment);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void BaseReviewValidationStrategy_ValidateComment_WithTooLongComment_ShouldReturnInvalidResult()
    {
        // Arrange
        var comment = new string('A', 1001); // 1001 characters, exceeds max of 1000
        var expectedError = "Comment must be between 10 and 1000 characters";

        // Act
        var result = TestBaseReviewValidationStrategy.PublicValidateComment(comment);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Theory]
    [InlineData(1, true)] // Minimum valid rating
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(5, true)] // Maximum valid rating
    public void BaseReviewValidationStrategy_IsValidRating_WithValidRatings_ShouldReturnTrue(int rating, bool expected)
    {
        // Act
        var result = TestBaseReviewValidationStrategy.PublicIsValidRating(rating);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, false)] // Below minimum
    [InlineData(-1, false)]
    [InlineData(6, false)] // Above maximum
    [InlineData(10, false)]
    public void BaseReviewValidationStrategy_IsValidRating_WithInvalidRatings_ShouldReturnFalse(int rating,
        bool expected)
    {
        // Act
        var result = TestBaseReviewValidationStrategy.PublicIsValidRating(rating);

        // Assert
        result.Should().Be(expected);
    }
}