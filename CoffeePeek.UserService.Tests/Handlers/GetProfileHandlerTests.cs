using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.UserService.Handlers;
using CoffeePeek.UserService.Models;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace CoffeePeek.UserService.Tests.Handlers;

public class GetProfileHandlerTests
{
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetProfileHandler _sut;

    public GetProfileHandlerTests()
    {
        _userRepositoryMock = new Mock<IGenericRepository<User>>();
        _redisServiceMock = new Mock<IRedisService>();
        _mapperMock = new Mock<IMapper>();

        var config = new TypeAdapterConfig();
        _mapperMock.Setup(x => x.Config).Returns(config);

        _sut = new GetProfileHandler(
            _userRepositoryMock.Object,
            _redisServiceMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithExistingUser_ReturnsUserProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDto
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com",
            PhotoUrl = "https://example.com/avatar.jpg",
            About = "Test bio",
            CreatedAt = DateTime.UtcNow
        };

        _redisServiceMock
            .Setup(x => x.GetAsync<UserDto>(It.IsAny<CacheKey>()))
            .ReturnsAsync((UserDto?)null);

        var mockQueryable = new List<User>
        {
            new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                AvatarUrl = "https://example.com/avatar.jpg",
                About = "Test bio"
            }
        }.AsQueryable().BuildMock();

        _userRepositoryMock
            .Setup(x => x.QueryAsNoTracking())
            .Returns(mockQueryable);

        var request = new GetProfileRequest(userId);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsError()
    {
        // Arrange
        _redisServiceMock
            .Setup(x => x.GetAsync<UserDto>(It.IsAny<CacheKey>()))
            .ReturnsAsync((UserDto?)null);

        var mockQueryable = new List<User>().AsQueryable().BuildMock();

        _userRepositoryMock
            .Setup(x => x.QueryAsNoTracking())
            .Returns(mockQueryable);

        var request = new GetProfileRequest(Guid.NewGuid());

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("User not found");
    }

    [Fact]
    public async Task Handle_WithCachedUser_ReturnsFromCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cachedUserDto = new UserDto
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com"
        };

        _redisServiceMock
            .Setup(x => x.GetAsync<UserDto>(It.IsAny<CacheKey>()))
            .ReturnsAsync(cachedUserDto);

        var request = new GetProfileRequest(userId);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        _userRepositoryMock.Verify(x => x.QueryAsNoTracking(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ReturnsError()
    {
        // Arrange
        _redisServiceMock
            .Setup(x => x.GetAsync<UserDto>(It.IsAny<CacheKey>()))
            .ReturnsAsync((UserDto?)null);

        var mockQueryable = new List<User>().AsQueryable().BuildMock();

        _userRepositoryMock
            .Setup(x => x.QueryAsNoTracking())
            .Returns(mockQueryable);

        var request = new GetProfileRequest(Guid.Empty);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }
}

public static class QueryableExtensions
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
                new[] { typeof(System.Linq.Expressions.Expression) })
            .MakeGenericMethod(resultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult });
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
        return new ValueTask();
    }
}