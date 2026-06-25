using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;
using FluentAssertions;
using Moq;

namespace CoffeePeek.Shops.Application.Tests.Features.Public.Feed;

public class GetCommunityFeedHandlerTests
{
    private readonly Mock<ICommunityFeedQueries> _repositoryMock = new();
    private readonly Mock<IQueryCommunityUserFollowRepository> _followRepoMock = new();
    private readonly Mock<IQueryCommunityCityFollowRepository> _cityFollowRepoMock = new();
    private readonly Mock<IQueryCoffeeShopRepository> _coffeeShopRepoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    [Fact]
    public async Task Handle_WhenCacheHit_ReturnsCachedResponseWithoutQueryingRepository()
    {
        var cached = new GetCommunityFeedResponse(
            [new CommunityFeedItemDto { Type = CommunityFeedItemType.Review, Id = Guid.NewGuid() }],
            TotalItems: 1,
            TotalPages: 1,
            CurrentPage: 1,
            PageSize: 20,
            Filter: CommunityFeedFilter.All,
            CityId: null);

        _cacheMock
            .Setup(c => c.GetAsync<GetCommunityFeedResponse>(It.IsAny<CacheKey>(), _ct))
            .ReturnsAsync(cached);

        var result = await GetCommunityFeedHandler.Handle(
            new GetCommunityFeedQuery(),
            _repositoryMock.Object,
            _followRepoMock.Object,
            _cityFollowRepoMock.Object,
            _coffeeShopRepoMock.Object,
            _cacheMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(cached);
        _repositoryMock.Verify(
            r => r.GetFeedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CommunityFeedFilter>(),
                It.IsAny<CommunityFeedQueryContext>(),
                _ct),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_QueriesRepositoryCachesAndReturnsPagedResponse()
    {
        var items = new List<CommunityFeedItemDto>
        {
            new()
            {
                Type = CommunityFeedItemType.CheckIn,
                Id = Guid.NewGuid(),
                ShopName = "Brew Lab"
            }
        };

        _cacheMock
            .Setup(c => c.GetAsync<GetCommunityFeedResponse>(It.IsAny<CacheKey>(), _ct))
            .ReturnsAsync((GetCommunityFeedResponse?)null);

        _repositoryMock
            .Setup(r => r.GetFeedAsync(
                1,
                20,
                CommunityFeedFilter.Reviews,
                It.Is<CommunityFeedQueryContext>(ctx => ctx.CityId == null && ctx.ViewerUserId == null),
                _ct))
            .ReturnsAsync((items, 25));

        var result = await GetCommunityFeedHandler.Handle(
            new GetCommunityFeedQuery(Page: 1, PageSize: 20, Filter: CommunityFeedFilter.Reviews),
            _repositoryMock.Object,
            _followRepoMock.Object,
            _cityFollowRepoMock.Object,
            _coffeeShopRepoMock.Object,
            _cacheMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        result.Data.TotalItems.Should().Be(25);
        result.Data.TotalPages.Should().Be(2);
        result.Data.CurrentPage.Should().Be(1);
        result.Data.Filter.Should().Be(CommunityFeedFilter.Reviews);

        _cacheMock.Verify(
            c => c.SetAsync(It.IsAny<CacheKey>(), It.IsAny<GetCommunityFeedResponse>(), It.IsAny<TimeSpan?>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClampsPageSizeToMaximumOf50()
    {
        _cacheMock
            .Setup(c => c.GetAsync<GetCommunityFeedResponse>(It.IsAny<CacheKey>(), _ct))
            .ReturnsAsync((GetCommunityFeedResponse?)null);

        _repositoryMock
            .Setup(r => r.GetFeedAsync(
                1,
                50,
                CommunityFeedFilter.All,
                It.IsAny<CommunityFeedQueryContext>(),
                _ct))
            .ReturnsAsync(([], 0));

        await GetCommunityFeedHandler.Handle(
            new GetCommunityFeedQuery(PageSize: 500),
            _repositoryMock.Object,
            _followRepoMock.Object,
            _cityFollowRepoMock.Object,
            _coffeeShopRepoMock.Object,
            _cacheMock.Object,
            _ct);

        _repositoryMock.Verify(
            r => r.GetFeedAsync(1, 50, CommunityFeedFilter.All, It.IsAny<CommunityFeedQueryContext>(), _ct),
            Times.Once);
    }

    [Fact]
    public async Task Handle_FollowingFilterWithoutViewer_ThrowsUnauthorized()
    {
        var act = async () => await GetCommunityFeedHandler.Handle(
            new GetCommunityFeedQuery(Filter: CommunityFeedFilter.Following),
            _repositoryMock.Object,
            _followRepoMock.Object,
            _cityFollowRepoMock.Object,
            _coffeeShopRepoMock.Object,
            _cacheMock.Object,
            _ct);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
