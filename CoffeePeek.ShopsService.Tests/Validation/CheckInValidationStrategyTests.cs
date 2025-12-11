using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy.CheckIn;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities.Shop;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoffeePeek.ShopsService.Tests.Validation;

public class CheckInValidationStrategyTests : IDisposable
{
    private readonly ShopsDbContext _dbContext;
    private readonly CheckInValidationStrategy _sut;
    private readonly Guid _validShopId;

    public CheckInValidationStrategyTests()
    {
        var options = new DbContextOptionsBuilder<ShopsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ShopsDbContext(options);
        
        // Seed test data
        _validShopId = Guid.NewGuid();
        _dbContext.Shops.Add(new Shop
        {
            Id = _validShopId,
            Name = "Test Coffee Shop",
            CityId = Guid.NewGuid()
        });
        _dbContext.SaveChanges();

        _sut = new CheckInValidationStrategy(_dbContext);
    }

    [Fact]
    public void Validate_WithValidRequest_ReturnsValid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Note = "Great coffee!"
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ReturnsInvalid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.Empty,
            ShopId = _validShopId,
            Note = "Test note"
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("UserId is required");
    }

    [Fact]
    public void Validate_WithEmptyShopId_ReturnsInvalid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = Guid.Empty,
            Note = "Test note"
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("ShopId is required");
    }

    [Fact]
    public void Validate_WithNonExistentShop_ReturnsInvalid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = Guid.NewGuid(), // Non-existent shop
            Note = "Test note"
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Shop not found");
    }

    [Fact]
    public void Validate_WithNoteTooLong_ReturnsInvalid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Note = new string('a', 501) // 501 characters
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("must not exceed 500 characters");
    }

    [Fact]
    public void Validate_WithNullNote_ReturnsValid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Note = null
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithReviewMissingHeader_ReturnsInvalid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Review = new CheckInReviewRequest
            {
                Header = "",
                Comment = "Great coffee experience!",
                RatingCoffee = 5,
                RatingPlace = 4,
                RatingService = 5
            }
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Review Header is required");
    }

    [Fact]
    public void Validate_WithReviewHeaderTooShort_ReturnsInvalid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Review = new CheckInReviewRequest
            {
                Header = "Ab",
                Comment = "Great coffee experience!",
                RatingCoffee = 5,
                RatingPlace = 4,
                RatingService = 5
            }
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("between 3 and 70 characters");
    }

    [Fact]
    public void Validate_WithReviewHeaderTooLong_ReturnsInvalid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Review = new CheckInReviewRequest
            {
                Header = new string('a', 71),
                Comment = "Great coffee experience!",
                RatingCoffee = 5,
                RatingPlace = 4,
                RatingService = 5
            }
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("between 3 and 70 characters");
    }

    [Fact]
    public void Validate_WithReviewCommentTooShort_ReturnsInvalid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Review = new CheckInReviewRequest
            {
                Header = "Good coffee",
                Comment = "Short",
                RatingCoffee = 5,
                RatingPlace = 4,
                RatingService = 5
            }
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("between 10 and 2000 characters");
    }

    [Fact]
    public void Validate_WithReviewCommentTooLong_ReturnsInvalid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Review = new CheckInReviewRequest
            {
                Header = "Good coffee",
                Comment = new string('a', 2001),
                RatingCoffee = 5,
                RatingPlace = 4,
                RatingService = 5
            }
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("between 10 and 2000 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Validate_WithInvalidRatingCoffee_ReturnsInvalid(int rating)
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Review = new CheckInReviewRequest
            {
                Header = "Good coffee",
                Comment = "Excellent experience overall",
                RatingCoffee = rating,
                RatingPlace = 4,
                RatingService = 5
            }
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("RatingCoffee must be between 1 and 5");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Validate_WithInvalidRatingPlace_ReturnsInvalid(int rating)
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Review = new CheckInReviewRequest
            {
                Header = "Good coffee",
                Comment = "Excellent experience overall",
                RatingCoffee = 5,
                RatingPlace = rating,
                RatingService = 5
            }
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("RatingPlace must be between 1 and 5");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Validate_WithInvalidRatingService_ReturnsInvalid(int rating)
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Review = new CheckInReviewRequest
            {
                Header = "Good coffee",
                Comment = "Excellent experience overall",
                RatingCoffee = 5,
                RatingPlace = 4,
                RatingService = rating
            }
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("RatingService must be between 1 and 5");
    }

    [Fact]
    public void Validate_WithCompleteValidReview_ReturnsValid()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = _validShopId,
            Note = "Quick visit before work",
            Review = new CheckInReviewRequest
            {
                Header = "Amazing coffee and atmosphere",
                Comment = "This place has the best espresso in town. The baristas are knowledgeable and friendly.",
                RatingCoffee = 5,
                RatingPlace = 4,
                RatingService = 5
            }
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}