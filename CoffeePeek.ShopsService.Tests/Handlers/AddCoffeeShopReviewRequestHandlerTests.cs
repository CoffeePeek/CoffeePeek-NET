using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities.Shop;
using CoffeePeek.ShopsService.Handlers.CoffeeShop.Review;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CoffeePeek.ShopsService.Tests.Handlers;

public class AddCoffeeShopReviewRequestHandlerTests : IDisposable
{
    private readonly ShopsDbContext _dbContext;
    private readonly Mock<IValidationStrategy<AddCoffeeShopReviewRequest>> _validationStrategyMock;
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly AddCoffeeShopReviewRequestHandler _sut;
    private readonly Guid _testShopId;
    private readonly Guid _testCityId;

    public AddCoffeeShopReviewRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ShopsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ShopsDbContext(options);
        _validationStrategyMock = new Mock<IValidationStrategy<AddCoffeeShopReviewRequest>>();
        _redisServiceMock = new Mock<IRedisService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        // Seed test data
        _testCityId = Guid.NewGuid();
        _testShopId = Guid.NewGuid();
        _dbContext.Shops.Add(new Shop
        {
            Id = _testShopId,
            Name = "Test Coffee Shop",
            CityId = _testCityId
        });
        _dbContext.SaveChanges();

        _sut = new AddCoffeeShopReviewRequestHandler(
            _dbContext,
            _validationStrategyMock.Object,
            _redisServiceMock.Object,
            _publishEndpointMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesReviewAndReturnsSuccess()
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            ShopId = _testShopId,
            UserId = Guid.NewGuid(),
            Header = "Great Coffee!",
            Comment = "Really enjoyed the atmosphere and quality of coffee.",
            RatingCoffee = 5,
            RatingPlace = 4,
            RatingService = 5
        };

        _validationStrategyMock
            .Setup(x => x.Validate(request))
            .Returns(new Abstractions.ValidationStrategy.ValidationResult { IsValid = true });

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ReviewId.Should().NotBeEmpty();

        var createdReview = await _dbContext.Reviews.FirstOrDefaultAsync();
        createdReview.Should().NotBeNull();
        createdReview!.Header.Should().Be(request.Header);
        createdReview.Comment.Should().Be(request.Comment);
        createdReview.RatingCoffee.Should().Be(request.RatingCoffee);
        createdReview.ShopId.Should().Be(_testShopId);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsError()
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            ShopId = _testShopId,
            UserId = Guid.NewGuid(),
            Header = "",
            Comment = "Comment",
            RatingCoffee = 5,
            RatingPlace = 4,
            RatingService = 5
        };

        _validationStrategyMock
            .Setup(x => x.Validate(request))
            .Returns(new Abstractions.ValidationStrategy.ValidationResult 
            { 
                IsValid = false, 
                ErrorMessage = "Header is required" 
            });

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Header is required");
    }

    [Fact]
    public async Task Handle_WithValidRequest_InvalidatesCache()
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            ShopId = _testShopId,
            UserId = Guid.NewGuid(),
            Header = "Great Coffee!",
            Comment = "Really enjoyed the atmosphere and quality of coffee.",
            RatingCoffee = 5,
            RatingPlace = 4,
            RatingService = 5
        };

        _validationStrategyMock
            .Setup(x => x.Validate(request))
            .Returns(new Abstractions.ValidationStrategy.ValidationResult { IsValid = true });

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        _redisServiceMock.Verify(
            x => x.RemoveAsync(It.IsAny<string>()),
            Times.AtLeastOnce
        );
        _redisServiceMock.Verify(
            x => x.RemoveByPatternAsync(It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithValidRequest_PublishesReviewAddedEvent()
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            ShopId = _testShopId,
            UserId = Guid.NewGuid(),
            Header = "Great Coffee!",
            Comment = "Really enjoyed the atmosphere and quality of coffee.",
            RatingCoffee = 5,
            RatingPlace = 4,
            RatingService = 5
        };

        _validationStrategyMock
            .Setup(x => x.Validate(request))
            .Returns(new Abstractions.ValidationStrategy.ValidationResult { IsValid = true });

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithValidRequest_SetsReviewDateToUtcNow()
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            ShopId = _testShopId,
            UserId = Guid.NewGuid(),
            Header = "Great Coffee!",
            Comment = "Really enjoyed the atmosphere and quality of coffee.",
            RatingCoffee = 5,
            RatingPlace = 4,
            RatingService = 5
        };

        _validationStrategyMock
            .Setup(x => x.Validate(request))
            .Returns(new Abstractions.ValidationStrategy.ValidationResult { IsValid = true });

        var beforeTime = DateTime.UtcNow.AddSeconds(-1);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        var afterTime = DateTime.UtcNow.AddSeconds(1);

        // Assert
        var createdReview = await _dbContext.Reviews.FirstOrDefaultAsync();
        createdReview.Should().NotBeNull();
        createdReview!.ReviewDate.Should().BeAfter(beforeTime);
        createdReview.ReviewDate.Should().BeBefore(afterTime);
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(3, 3, 3)]
    [InlineData(5, 5, 5)]
    [InlineData(2, 4, 3)]
    public async Task Handle_WithVariousRatings_SavesCorrectly(int ratingCoffee, int ratingPlace, int ratingService)
    {
        // Arrange
        var request = new AddCoffeeShopReviewRequest
        {
            ShopId = _testShopId,
            UserId = Guid.NewGuid(),
            Header = "Review",
            Comment = "Test comment for various ratings.",
            RatingCoffee = ratingCoffee,
            RatingPlace = ratingPlace,
            RatingService = ratingService
        };

        _validationStrategyMock
            .Setup(x => x.Validate(request))
            .Returns(new Abstractions.ValidationStrategy.ValidationResult { IsValid = true });

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var createdReview = await _dbContext.Reviews.FirstOrDefaultAsync();
        createdReview!.RatingCoffee.Should().Be(ratingCoffee);
        createdReview.RatingPlace.Should().Be(ratingPlace);
        createdReview.RatingService.Should().Be(ratingService);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}