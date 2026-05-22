using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shops.Application.Common.Responses;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using FluentAssertions;
using Moq;

namespace CoffeePeek.Shops.Application.Tests.Features.CoffeeShop.SearchCoffeeShops;

public class SearchCoffeeShopsHandlerTests
{
    private readonly Mock<ICoffeeShopQueries> _shopQueriesMock = new();
    private readonly Mock<IUserFavoriteRepository> _favoriteRepoMock = new();
    private readonly Mock<IQueryCheckInRepository> _visitRepoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    private static SearchCoffeeShopsQuery BuildQuery(Guid? userId = null)
        => new SearchCoffeeShopsQuery(UserId: userId);

    [Fact]
    public async Task Handle_AnonymousRequest_ReturnsCachedResponse()
    {
        var shopId = Guid.NewGuid();
        var shop = new ShortShopDto { Id = shopId, Name = "Test Shop" };
        var expectedResponse = new GetCoffeeShopsResponse
        {
            CoffeeShops = [shop],
            TotalItems = 1,
            CurrentPage = 1,
            TotalPages = 1
        };

        _cacheMock
            .Setup(c => c.GetAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<CancellationToken, Task<GetCoffeeShopsResponse>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var query = BuildQuery(userId: null);
        var result = await SearchCoffeeShopsHandler.Handle(
            query,
            _shopQueriesMock.Object,
            _favoriteRepoMock.Object,
            _visitRepoMock.Object,
            _cacheMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data.CoffeeShops.Should().HaveCount(1);
        _favoriteRepoMock.Verify(
            r => r.GetFavoriteShopIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _visitRepoMock.Verify(
            r => r.GetVisitedShopIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_AuthenticatedRequest_EnrichesWithFavoriteAndVisitedFlags()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var shop = new ShortShopDto { Id = shopId, Name = "Test Shop" };
        var response = new GetCoffeeShopsResponse
        {
            CoffeeShops = [shop],
            TotalItems = 1,
            CurrentPage = 1,
            TotalPages = 1
        };

        _cacheMock
            .Setup(c => c.GetAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<CancellationToken, Task<GetCoffeeShopsResponse>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        _favoriteRepoMock
            .Setup(r => r.GetFavoriteShopIdsAsync(userId, _ct))
            .ReturnsAsync(new List<Guid> { shopId });

        _visitRepoMock
            .Setup(r => r.GetVisitedShopIdsAsync(userId, _ct))
            .ReturnsAsync(new List<Guid>());

        var query = BuildQuery(userId: userId);
        var result = await SearchCoffeeShopsHandler.Handle(
            query,
            _shopQueriesMock.Object,
            _favoriteRepoMock.Object,
            _visitRepoMock.Object,
            _cacheMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data.CoffeeShops[0].IsFavorite.Should().BeTrue();
        result.Data.CoffeeShops[0].IsVisited.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenCacheReturnsNull_ReturnsError()
    {
        _cacheMock
            .Setup(c => c.GetAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<CancellationToken, Task<GetCoffeeShopsResponse>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCoffeeShopsResponse?)null);

        var query = BuildQuery(userId: null);
        var result = await SearchCoffeeShopsHandler.Handle(
            query,
            _shopQueriesMock.Object,
            _favoriteRepoMock.Object,
            _visitRepoMock.Object,
            _cacheMock.Object,
            _ct);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Failed to retrieve");
    }
}
