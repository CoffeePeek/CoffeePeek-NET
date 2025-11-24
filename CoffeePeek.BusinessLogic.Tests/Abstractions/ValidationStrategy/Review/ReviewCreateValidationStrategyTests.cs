using CoffeePeek.BusinessLogic.Abstractions.Review;
using CoffeePeek.Contract.Requests.CoffeeShop;
using FluentAssertions;

namespace CoffeePeek.BusinessLogic.Tests.Abstractions.ValidationStrategy.Review;

public class ReviewCreateValidationStrategyTests
{
    private readonly ReviewCreateValidationStrategy _validator = new();

    [Fact]
    public void ReviewCreateValidationStrategy_Validate_WithValidReview_ShouldReturnValidResult()
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great coffee shop",
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData(0, "ShopId must be greater than 0")]
    [InlineData(-1, "ShopId must be greater than 0")]
    public void ReviewCreateValidationStrategy_Validate_WithInvalidShopId_ShouldReturnInvalidResult(int shopId,
        string expectedError)
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = shopId,
            Header = "Great coffee shop",
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Theory]
    [InlineData(0, "UserId must be greater than 0")]
    [InlineData(-1, "UserId must be greater than 0")]
    public void ReviewCreateValidationStrategy_Validate_WithInvalidUserId_ShouldReturnInvalidResult(int userId,
        string expectedError)
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            UserId = userId,
            ShopId = 1,
            Header = "Great coffee shop",
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Theory]
    [InlineData(null, "Header is required")]
    [InlineData("", "Header is required")]
    [InlineData("  ", "Header is required")]
    [InlineData("AB", "Header must be between 3 and 100 characters")] // Too short
    [InlineData("A", "Header must be between 3 and 100 characters")] // Too short
    public void ReviewCreateValidationStrategy_Validate_WithInvalidHeader_ShouldReturnInvalidResult(string header,
        string expectedError)
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = header,
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Fact]
    public void ReviewCreateValidationStrategy_Validate_WithHeaderAtBoundaryLengths_ShouldReturnValidResult()
    {
        // Test minimum valid length (3 characters)
        var request1 = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "ABC", // Exactly 3 characters
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };

        var result1 = _validator.Validate(request1);
        result1.IsValid.Should().BeTrue();

        // Test maximum valid length (100 characters)
        var request2 = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = new string('A', 100), // Exactly 100 characters
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };

        var result2 = _validator.Validate(request2);
        result2.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null, "Comment is required")]
    [InlineData("", "Comment is required")]
    [InlineData("  ", "Comment is required")]
    [InlineData("Too short", "Comment must be between 10 and 1000 characters")] // Too short (9 chars)
    public void ReviewCreateValidationStrategy_Validate_WithInvalidComment_ShouldReturnInvalidResult(string comment,
        string expectedError)
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great coffee shop",
            Comment = comment,
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Fact]
    public void ReviewCreateValidationStrategy_Validate_WithCommentAtBoundaryLengths_ShouldReturnValidResult()
    {
        // Test minimum valid length (10 characters)
        var request1 = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great coffee shop",
            Comment = "1234567890", // Exactly 10 characters
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };

        var result1 = _validator.Validate(request1);
        result1.IsValid.Should().BeTrue();

        // Test maximum valid length (1000 characters)
        var request2 = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great coffee shop",
            Comment = new string('A', 1000), // Exactly 1000 characters
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };

        var result2 = _validator.Validate(request2);
        result2.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, "RatingCoffee must be between 1 and 5")]
    [InlineData(6, "RatingCoffee must be between 1 and 5")]
    [InlineData(-1, "RatingCoffee must be between 1 and 5")]
    public void ReviewCreateValidationStrategy_Validate_WithInvalidRatingCoffee_ShouldReturnInvalidResult(int rating,
        string expectedError)
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great coffee shop",
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = rating,
            RatingService = 4,
            RatingPlace = 5
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Theory]
    [InlineData(0, "RatingService must be between 1 and 5")]
    [InlineData(6, "RatingService must be between 1 and 5")]
    [InlineData(-1, "RatingService must be between 1 and 5")]
    public void ReviewCreateValidationStrategy_Validate_WithInvalidRatingService_ShouldReturnInvalidResult(int rating,
        string expectedError)
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great coffee shop",
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = 5,
            RatingService = rating,
            RatingPlace = 5
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Theory]
    [InlineData(0, "RatingPlace must be between 1 and 5")]
    [InlineData(6, "RatingPlace must be between 1 and 5")]
    [InlineData(-1, "RatingPlace must be between 1 and 5")]
    public void ReviewCreateValidationStrategy_Validate_WithInvalidRatingPlace_ShouldReturnInvalidResult(int rating,
        string expectedError)
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great coffee shop",
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = rating
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Fact]
    public void ReviewCreateValidationStrategy_Validate_WithAllRatingsAtBoundaryValues_ShouldReturnValidResult()
    {
        // Test minimum valid rating (1)
        var request1 = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great coffee shop",
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = 1, // Minimum valid
            RatingService = 1, // Minimum valid
            RatingPlace = 1 // Minimum valid
        };

        var result1 = _validator.Validate(request1);
        result1.IsValid.Should().BeTrue();

        // Test maximum valid rating (5)
        var request2 = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great coffee shop",
            Comment = "This is a wonderful place to enjoy coffee with friends.",
            RatingCoffee = 5, // Maximum valid
            RatingService = 5, // Maximum valid
            RatingPlace = 5 // Maximum valid
        };

        var result2 = _validator.Validate(request2);
        result2.IsValid.Should().BeTrue();
    }
}