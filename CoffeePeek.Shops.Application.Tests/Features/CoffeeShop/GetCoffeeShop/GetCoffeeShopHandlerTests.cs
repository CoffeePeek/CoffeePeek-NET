using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CoffeePeek.Shops.Application.Tests.Features.CoffeeShop.GetCoffeeShop;

public class GetCoffeeShopHandlerTests
{
    private readonly Mock<ICoffeeShopQueries> _shopQueriesMock = new();
    private readonly Mock<IUserFavoriteRepository> _favoriteRepoMock = new();
    private readonly Mock<IQueryCheckInRepository> _checkInRepoMock = new();
    private readonly Mock<IQueryReviewRepository> _reviewRepoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    private static CoffeeShopDetailsDto BuildShopDto(Guid shopId)
        => new CoffeeShopDetailsDto
        {
            Id = shopId,
            Name = "Test Coffee Shop",
            Reviews = Array.Empty<ReviewDto>()
        };

    [Fact]
    public async Task Handle_AnonymousRequest_ReturnsShopWithoutPersonalization()
    {
        var shopId = Guid.NewGuid();
        var shopDto = BuildShopDto(shopId);

        _cacheMock
            .Setup(c => c.GetAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<CancellationToken, Task<CoffeeShopDetailsDto>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shopDto);

        var query = new GetCoffeeShopQuery(shopId, UserId: null);
        var result = await GetCoffeeShopHandler.Handle(
            query,
            _shopQueriesMock.Object,
            _favoriteRepoMock.Object,
            _checkInRepoMock.Object,
            _reviewRepoMock.Object,
            _cacheMock.Object,
            _mapperMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data.ShopDto.Id.Should().Be(shopId);
        _favoriteRepoMock.Verify(
            r => r.Exists(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _checkInRepoMock.Verify(
            r => r.Exists(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_AuthenticatedRequest_EnrichesWithFavoriteAndVisited()
    {
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shopDto = BuildShopDto(shopId);

        _cacheMock
            .Setup(c => c.GetAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<CancellationToken, Task<CoffeeShopDetailsDto>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shopDto);

        _favoriteRepoMock
            .Setup(r => r.Exists(userId, shopId, _ct))
            .ReturnsAsync(true);

        _checkInRepoMock
            .Setup(r => r.Exists(userId, shopId, _ct))
            .ReturnsAsync(false);

        var query = new GetCoffeeShopQuery(shopId, UserId: userId);
        var result = await GetCoffeeShopHandler.Handle(
            query,
            _shopQueriesMock.Object,
            _favoriteRepoMock.Object,
            _checkInRepoMock.Object,
            _reviewRepoMock.Object,
            _cacheMock.Object,
            _mapperMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data.ShopDto.IsFavorite.Should().BeTrue();
        result.Data.ShopDto.IsVisited.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenShopNotFound_ReturnsError()
    {
        var shopId = Guid.NewGuid();

        _cacheMock
            .Setup(c => c.GetAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<CancellationToken, Task<CoffeeShopDetailsDto>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CoffeeShopDetailsDto?)null);

        var query = new GetCoffeeShopQuery(shopId, UserId: null);
        var result = await GetCoffeeShopHandler.Handle(
            query,
            _shopQueriesMock.Object,
            _favoriteRepoMock.Object,
            _checkInRepoMock.Object,
            _reviewRepoMock.Object,
            _cacheMock.Object,
            _mapperMock.Object,
            _ct);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Shop not found");
    }
}
