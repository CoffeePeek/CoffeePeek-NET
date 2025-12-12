using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Handlers.CoffeeShop.CheckIn;
using CoffeePeek.ShopsService.Services.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ValidationResult = CoffeePeek.ShopsService.Abstractions.ValidationStrategy.ValidationResult;

namespace CoffeePeek.ShopsService.Tests.Handlers;

public class CreateCheckInHandlerTests : IDisposable
{
    private readonly ShopsDbContext _dbContext;
    private readonly Mock<IValidationStrategy<CreateCheckInRequest>> _validationMock = new();
    private readonly Mock<IRedisService> _redisMock = new();
    private readonly Mock<IPublishEndpoint> _publishMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly CreateCheckInHandler _sut;

    public CreateCheckInHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ShopsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ShopsDbContext(options);
        _sut = new CreateCheckInHandler(
            _dbContext,
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
        _publishMock.Verify(x => x.Publish(It.IsAny<CheckinCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
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
        _publishMock.Verify(x => x.Publish(It.IsAny<CheckinCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _publishMock.Verify(x => x.Publish(It.IsAny<ReviewAddedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
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
            .ReturnsAsync(new[]
            {
                new Contract.Dtos.Internal.CityDto { Id = shopId, Name = "City" }
            });

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CheckInId.Should().NotBeEmpty();
        result.Data.ReviewId.Should().NotBeNull();

        _dbContext.CheckIns.Count().Should().Be(1);
        _dbContext.Reviews.Count().Should().Be(1);

        _publishMock.Verify(x => x.Publish(It.IsAny<CheckinCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _publishMock.Verify(x => x.Publish(It.IsAny<ReviewAddedEvent>(), It.IsAny<CancellationToken>()), Times.Once);

        _redisMock.Verify(x => x.RemoveAsync(CacheKey.Shop.ById(shopId)), Times.Exactly(2));
        _redisMock.Verify(x => x.RemoveByPatternAsync(CacheKey.Shop.ByCityPattern(shopId)), Times.Exactly(2));
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

