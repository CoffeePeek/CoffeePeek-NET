using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.ShopsService.Configuration;
using CoffeePeek.ShopsService.Entities;
using CoffeePeek.ShopsService.Handlers.CoffeeShop;
using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace CoffeePeek.ShopsService.Tests.Handlers;

public class GetCoffeeShopHandlerTests
{
    private readonly Mock<IGenericRepository<Shop>> _shopRepositoryMock = new();
    private readonly Mock<IRedisService> _redisServiceMock = new();
    private readonly IMapper _mapper = MapsterConfiguration.CreateMapper();
    private readonly GetCoffeeShopHandler _sut;

    public GetCoffeeShopHandlerTests()
    {
        _sut = new GetCoffeeShopHandler(
            _shopRepositoryMock.Object,
            _mapper,
            _redisServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithCachedResponse_ReturnsFromCache()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var cachedResponse = Response<GetCoffeeShopResponse>.Success(
            new GetCoffeeShopResponse(new ShopDto { Id = shopId, Name = "Cached" }));

        _redisServiceMock
            .Setup(r => r.GetAsync<Response<GetCoffeeShopResponse>>(CacheKey.Shop.ById(shopId)))
            .ReturnsAsync(cachedResponse);

        var request = new GetCoffeeShopCommand(shopId);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(cachedResponse);
        _shopRepositoryMock.Verify(r => r.QueryAsNoTracking(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingShop_ReturnsShopAndCachesResult()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var shops = new List<Shop>
        {
            new()
            {
                Id = shopId,
                Name = "Test Shop",
                LocationId = Guid.NewGuid(),
                Location = new Location
                {
                    Id = Guid.NewGuid(),
                    Address = "Test address",
                    Latitude = 10,
                    Longitude = 20,
                    ShopId = shopId
                }
            }
        }.AsQueryable().BuildMock();

        _redisServiceMock
            .Setup(r => r.GetAsync<Response<GetCoffeeShopResponse>>(It.IsAny<CacheKey>()))
            .ReturnsAsync((Response<GetCoffeeShopResponse>?)null);

        _shopRepositoryMock
            .Setup(r => r.QueryAsNoTracking())
            .Returns(shops);

        var request = new GetCoffeeShopCommand(shopId);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Shop.Should().NotBeNull();
        result.Data.Shop.Id.Should().Be(shopId);

        _redisServiceMock.Verify(r => r.SetAsync(CacheKey.Shop.ById(shopId), It.IsAny<Response<GetCoffeeShopResponse>>(), null), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMissingShop_ReturnsErrorAndDoesNotCache()
    {
        // Arrange
        var request = new GetCoffeeShopCommand(Guid.NewGuid());

        _redisServiceMock
            .Setup(r => r.GetAsync<Response<GetCoffeeShopResponse>>(It.IsAny<CacheKey>()))
            .ReturnsAsync((Response<GetCoffeeShopResponse>?)null);

        _shopRepositoryMock
            .Setup(r => r.QueryAsNoTracking())
            .Returns(new List<Shop>().AsQueryable().BuildMock());

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _redisServiceMock.Verify(r => r.SetAsync(It.IsAny<CacheKey>(), It.IsAny<Response<GetCoffeeShopResponse>>(), null), Times.Never);
    }
}

internal static class QueryableExtensions
{
    public static IQueryable<T> BuildMock<T>(this IQueryable<T> source)
    {
        var mock = new Mock<IQueryable<T>>();
        mock.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(source.GetEnumerator()));
        mock.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(source.Provider));
        mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
        mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
        mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(source.GetEnumerator());
        return mock.Object;
    }
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression,
        CancellationToken cancellationToken)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                nameof(IQueryProvider.Execute),
                1,
                new[] { typeof(System.Linq.Expressions.Expression) })!
            .MakeGenericMethod(resultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

