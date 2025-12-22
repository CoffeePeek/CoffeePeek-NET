using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Outbox;
using CoffeePeek.Shared.Infrastructure.Persistence.Data;
using CoffeePeek.Shops.Application.Handlers.CoffeeShop.Review;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Infrastructure.Configuration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ValidationResult = CoffeePeek.Shops.Application.ValidationResult;

namespace CoffeePeek.ShopsService.Tests.Handlers;

public class AddCoffeeShopReviewRequestHandlerTests : IDisposable
{
    private readonly ShopsDbContext _dbContext;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IValidationStrategy<AddCoffeeShopReviewRequest>> _validationStrategyMock;
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly Mock<IOutboxEventPublisher> _publishEndpointMock;
    private readonly IGenericRepository<Shop> _shopRepository;
    private readonly IGenericRepository<Review> _reviewRepository;
    private readonly AddCoffeeShopReviewRequestHandler _sut;
    private readonly Guid _testShopId;

    public AddCoffeeShopReviewRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ShopsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ShopsDbContext(options);
        _validationStrategyMock = new Mock<IValidationStrategy<AddCoffeeShopReviewRequest>>();
        _redisServiceMock = new Mock<IRedisService>();
        _publishEndpointMock = new Mock<IOutboxEventPublisher>();

        // Use real repositories with InMemory DB instead of mocks to support EF async operations
        _shopRepository = new GenericRepository<Shop, ShopsDbContext>(_dbContext);
        _reviewRepository = new GenericRepository<Review, ShopsDbContext>(_dbContext);
        
        // Setup UnitOfWork mock to actually save changes to InMemory DB
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) => 
            {
                await _dbContext.SaveChangesAsync(ct);
                return 1;
            });

        // Seed test data
        var testCityId = Guid.NewGuid();
        _testShopId = Guid.NewGuid();
        _dbContext.Shops.Add(new Shop
        {
            Id = _testShopId,
            Name = "Test Coffee Shop",
            CityId = testCityId
        });
        _dbContext.SaveChanges();

        _sut = new AddCoffeeShopReviewRequestHandler(
            _reviewRepository,
            _shopRepository,
            _unitOfWorkMock.Object,
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
            .Returns(ValidationResult.Valid);

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
            .Returns(ValidationResult.Invalid("Header is required"));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Header is required");
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
            .Returns(ValidationResult.Valid);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        _redisServiceMock.Verify(
            x => x.RemoveAsync(It.IsAny<CoffeePeek.Shared.Infrastructure.Cache.CacheKey>()),
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
            .Returns(ValidationResult.Valid);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.PublishAsync(It.IsAny<ReviewAddedEvent>(), It.IsAny<CancellationToken>()),
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
            .Returns(ValidationResult.Valid);

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
            .Returns(ValidationResult.Valid);

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