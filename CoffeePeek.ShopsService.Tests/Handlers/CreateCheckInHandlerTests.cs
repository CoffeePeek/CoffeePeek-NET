using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Outbox;
using CoffeePeek.Shared.Infrastructure.Persistence.Data;
using CoffeePeek.Shops.Application.Handlers.CoffeeShop.CheckIn;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Infrastructure.Configuration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ValidationResult = CoffeePeek.Shops.Application.ValidationResult;

namespace CoffeePeek.ShopsService.Tests.Handlers;

public class CreateCheckInHandlerTests : IDisposable
{
    private readonly ShopsDbContext _dbContext;
    private readonly IGenericRepository<CheckIn> _checkInRepository;
    private readonly IGenericRepository<Review> _reviewRepository;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IValidationStrategy<CreateCheckInRequest>> _validationMock = new();
    private readonly Mock<IRedisService> _redisMock = new();
    private readonly Mock<IOutboxEventPublisher> _publishMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly CreateCheckInHandler _sut;

    public CreateCheckInHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ShopsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ShopsDbContext(options);
        
        // Use real repositories with InMemory DB instead of mocks to support EF async operations
        _checkInRepository = new GenericRepository<CheckIn, ShopsDbContext>(_dbContext);
        _reviewRepository = new GenericRepository<Review, ShopsDbContext>(_dbContext);
        
        // Setup UnitOfWork mock to actually save changes to InMemory DB
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) => 
            {
                await _dbContext.SaveChangesAsync(ct);
                return 1;
            });
        
        _sut = new CreateCheckInHandler(
            _checkInRepository,
            _reviewRepository,
            _unitOfWorkMock.Object,
            _validationMock.Object,
            _redisMock.Object,
            _publishMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsError()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = Guid.NewGuid(),
            Note = "note"
        };

        _validationMock.Setup(v => v.Validate(request)).Returns(ValidationResult.Invalid("invalid"));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("invalid");
        _dbContext.CheckIns.Should().BeEmpty();
        _publishMock.Verify(x => x.PublishAsync(It.IsAny<CheckinCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesCheckInAndPublishesEvent()
    {
        // Arrange
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = Guid.NewGuid(),
            Note = "note"
        };

        _validationMock.Setup(v => v.Validate(request)).Returns(ValidationResult.Valid);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CheckInId.Should().NotBeEmpty();
        result.Data.ReviewId.Should().BeNull();

        _dbContext.CheckIns.Count().Should().Be(1);
        _publishMock.Verify(x => x.PublishAsync(It.IsAny<CheckinCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _publishMock.Verify(x => x.PublishAsync(It.IsAny<ReviewAddedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _redisMock.Verify(x => x.RemoveAsync(It.IsAny<CacheKey>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithReview_CreatesReview_CheckIn_InvalidatesCache_AndPublishesEvents()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var request = new CreateCheckInRequest
        {
            UserId = Guid.NewGuid(),
            ShopId = shopId,
            Note = "note",
            Review = new CheckInReviewRequest
            {
                Header = "hdr",
                Comment = "cmt",
                RatingCoffee = 5,
                RatingPlace = 4,
                RatingService = 5
            }
        };

        _validationMock.Setup(v => v.Validate(request)).Returns(ValidationResult.Valid);

        _cacheServiceMock
            .Setup(c => c.GetCities())
            .ReturnsAsync([
                new Contract.Dtos.Internal.CityDto { Id = shopId, Name = "City" }
            ]);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CheckInId.Should().NotBeEmpty();
        result.Data.ReviewId.Should().NotBeNull();

        _dbContext.CheckIns.Count().Should().Be(1);
        _dbContext.Reviews.Count().Should().Be(1);

        _publishMock.Verify(x => x.PublishAsync(It.IsAny<CheckinCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _publishMock.Verify(x => x.PublishAsync(It.IsAny<ReviewAddedEvent>(), It.IsAny<CancellationToken>()), Times.Once);

        _redisMock.Verify(x => x.RemoveAsync(CacheKey.CachedShop.ById(shopId)), Times.Exactly(2));
        _redisMock.Verify(x => x.RemoveByPatternAsync(CacheKey.CachedShop.ByCityPattern(shopId)), Times.Exactly(2));
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

