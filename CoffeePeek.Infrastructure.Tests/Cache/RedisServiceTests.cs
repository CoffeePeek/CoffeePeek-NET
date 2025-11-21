using CoffeePeek.Infrastructure.Cache;
using FluentAssertions;
using Moq;
using StackExchange.Redis;
using System.Text.Json;

namespace CoffeePeek.Infrastructure.Tests.Cache;

public class RedisServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly RedisService _service;

    public RedisServiceTests()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);
        _service = new RedisService(_redisMock.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDeserializedValue_WhenKeyExists()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestData { Id = 1, Name = "Test" };
        var serialized = JsonSerializer.Serialize(testObject);

        _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serialized));

        // Act
        var result = await _service.GetAsync<TestData>(key);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDefault_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";

        _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _service.GetAsync<TestData>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task TryGetAsync_ShouldReturnSuccessAndValue_WhenKeyExists()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestData { Id = 2, Name = "Test2" };
        var serialized = JsonSerializer.Serialize(testObject);

        _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serialized));

        // Act
        var (success, value) = await _service.TryGetAsync<TestData>(key);

        // Assert
        success.Should().BeTrue();
        value.Should().NotBeNull();
        value!.Id.Should().Be(2);
        value.Name.Should().Be("Test2");
    }

    [Fact]
    public async Task TryGetAsync_ShouldReturnFailure_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";

        _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var (success, value) = await _service.TryGetAsync<TestData>(key);

        // Assert
        success.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public async Task TryGetAsync_ShouldReturnFailure_WhenJsonDeserializationFails()
    {
        // Arrange
        var key = "invalid-json-key";

        _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue("invalid-json"));

        // Act
        var (success, value) = await _service.TryGetAsync<TestData>(key);

        // Assert
        success.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public async Task TryGetAsync_ShouldReturnFailure_WhenRedisConnectionFails()
    {
        // Arrange
        var key = "test-key";

        _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Connection failed"));

        // Act
        var (success, value) = await _service.TryGetAsync<TestData>(key);

        // Assert
        success.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public async Task GetAsyncById_ShouldReturnValue_WhenIdExists()
    {
        // Arrange
        var id = 123;
        var testObject = new TestData { Id = id, Name = "ById" };
        var serialized = JsonSerializer.Serialize(testObject);
        var expectedKey = $"{nameof(TestData)}-{id}";

        _databaseMock.Setup(d => d.StringGetAsync(expectedKey, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serialized));

        // Act
        var result = await _service.GetAsyncById<TestData>(id.ToString());

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(123);
        result.Name.Should().Be("ById");
    }

    [Fact]
    public async Task SetAsync_ShouldCallRedisWithCorrectParameters()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestData { Id = 1, Name = "Set" };
        var expiry = TimeSpan.FromMinutes(10);

        // Act
        await _service.SetAsync(key, testObject, expiry);

        // Assert
        _databaseMock.Verify(
            d => d.StringSetAsync(
                key,
                It.Is<RedisValue>(v => v.ToString().Contains("Set")),
                expiry,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldCallRedisWithoutExpiry_WhenExpiryIsNull()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestData { Id = 1, Name = "NoExpiry" };

        // Act
        await _service.SetAsync(key, testObject);

        // Assert
        _databaseMock.Verify(
            d => d.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                null,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallKeyDelete()
    {
        // Arrange
        var key = "test-key";

        _databaseMock.Setup(d => d.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _service.RemoveAsync(key);

        // Assert
        _databaseMock.Verify(
            d => d.KeyDeleteAsync(key, It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Theory]
    [InlineData("key1")]
    [InlineData("user:123")]
    [InlineData("cache_item")]
    public async Task RemoveAsync_ShouldHandleVariousKeys(string key)
    {
        // Arrange
        _databaseMock.Setup(d => d.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _service.RemoveAsync(key);

        // Assert
        _databaseMock.Verify(
            d => d.KeyDeleteAsync(key, It.IsAny<CommandFlags>()),
            Times.Once);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
