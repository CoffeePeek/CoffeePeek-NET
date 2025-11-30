using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Review;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.CoffeeShop.Review;

public class UpdateCoffeeShopReviewRequestHandlerTests
{
    private readonly Mock<CoffeePeekDbContext> _dbContextMock;
    private readonly Mock<DbSet<Domain.Entities.Review.Review>> _reviewsDbSetMock;
    private readonly UpdateCoffeeShopReviewRequestHandler _handler;

    public UpdateCoffeeShopReviewRequestHandlerTests()
    {
        // Setup mocks
        _dbContextMock = new Mock<CoffeePeekDbContext>();
        _reviewsDbSetMock = new Mock<DbSet<Domain.Entities.Review.Review>>();
        var validationStrategy = new Mock<IValidationStrategy<UpdateCoffeeShopReviewRequest>>();

        // Setup DbContext mock
        _dbContextMock.Setup(db => db.Reviews).Returns(_reviewsDbSetMock.Object);

        // Create handler instance
        _handler = new UpdateCoffeeShopReviewRequestHandler(_dbContextMock.Object, validationStrategy.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenReviewNotFound()
    {
        // Arrange
        var request = new UpdateCoffeeShopReviewRequest
        {
            UserId =
                999,
            Header =
                "Updated Header",
            Comment =
                "Updated Comment",
            RatingCoffee =
                5,
            RatingPlace =
                4,
            RatingService =
                5
        };

        Domain.Entities.Review.Review? review = null;

        _reviewsDbSetMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Review.Review, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Review not found");

        _reviewsDbSetMock.Verify(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Review.Review, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenReviewIsUpdated()
    {
        // Arrange
        var request = new UpdateCoffeeShopReviewRequest
        {
            UserId = 1,
            Header = "Updated Header",
            Comment = "Updated Comment",
            RatingCoffee = 5,
            RatingPlace = 4,
            RatingService = 5
        };

        var review = new Domain.Entities.Review.Review
        {
            Id = 1,
            Header = "Old Header",
            Comment = "Old Comment",
            RatingCoffee = 3,
            RatingPlace = 3,
            RatingService = 3
        };

        _reviewsDbSetMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Review.Review, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Review updated successfully");

        _reviewsDbSetMock.Verify(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Review.Review, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        review.Header.Should().Be("Updated Header");
        review.Comment.Should().Be("Updated Comment");
        review.RatingCoffee.Should().Be(5);
        review.RatingPlace.Should().Be(4);
        review.RatingService.Should().Be(5);

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}