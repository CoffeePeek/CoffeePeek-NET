using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.BusinessLogic.Abstractions.Review;
using CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Domain.Databases;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.CoffeeShop.Review;

public class AddCoffeeShopReviewRequestHandlerTests
{
    private readonly Mock<CoffeePeekDbContext> _dbContextMock;
    private readonly Mock<DbSet<Domain.Entities.Review.Review>> _reviewsDbSetMock;
    private readonly Mock<ReviewCreateValidationStrategy> _validationStrategyMock;
    private readonly AddCoffeeShopReviewRequestHandler _handler;

    public AddCoffeeShopReviewRequestHandlerTests()
    {
        // Setup mocks
        _dbContextMock = new Mock<CoffeePeekDbContext>();
        _reviewsDbSetMock = new Mock<DbSet<Domain.Entities.Review.Review>>();
        _validationStrategyMock = new Mock<ReviewCreateValidationStrategy>();

        // Setup DbContext mock
        _dbContextMock.Setup(db => db.Reviews).Returns(_reviewsDbSetMock.Object);

        // Create handler instance
        _handler = new AddCoffeeShopReviewRequestHandler(_dbContextMock.Object, _validationStrategyMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenValidationFails()
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great Coffee",
            Comment = "Loved it!",
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };
            
        var validationError = "Comment is required";
        var validationResult = ValidationResult.Invalid(validationError);

        _validationStrategyMock
            .Setup(v => v.Validate(request))
            .Returns(validationResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(validationError);

        _validationStrategyMock.Verify(v => v.Validate(request), Times.Once);
        _reviewsDbSetMock.Verify(r => r.Add(It.IsAny<Domain.Entities.Review.Review>()), Times.Never);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenReviewIsAdded()
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            UserId = 1,
            ShopId = 1,
            Header = "Great Coffee",
            Comment = "Loved it!",
            RatingCoffee = 5,
            RatingService = 4,
            RatingPlace = 5
        };
            
        var validationResult = ValidationResult.Valid;

        _validationStrategyMock
            .Setup(v => v.Validate(request))
            .Returns(validationResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ReviewId.Should().BeGreaterThan(0);

        _validationStrategyMock.Verify(v => v.Validate(request), Times.Once);
        _reviewsDbSetMock.Verify(r => r.Add(It.IsAny<Domain.Entities.Review.Review>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}