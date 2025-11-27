using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Domain.UnitOfWork;
using CoffeePeek.Infrastructure.Cache;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CoffeePeek.Infrastructure.Tests.Cache;

public class CacheServiceTests
{
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRepository<City>> _cityRepositoryMock;
    private readonly CacheService _service;

    public CacheServiceTests()
    {
        _redisServiceMock = new Mock<IRedisService>();
        _mapperMock = new Mock<IMapper>();
        _cityRepositoryMock = new Mock<IRepository<City>>();
        _service = new CacheService(
            _redisServiceMock.Object,
            _mapperMock.Object,
            _cityRepositoryMock.Object);
    }

    [Fact]
    public async Task GetCities_ShouldReturnCachedData_WhenCacheHit()
    {
        // Arrange
        var cachedCities = new List<CityDto>
        {
            new CityDto { Id = 1, Name = "City1" },
            new CityDto { Id = 2, Name = "City2" }
        };

        _redisServiceMock
            .Setup(r => r.TryGetAsync<ICollection<CityDto>>(It.IsAny<string>()))
            .ReturnsAsync((true, cachedCities));

        // Act
        var result = await _service.GetCities();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(cachedCities);

        _cityRepositoryMock.Verify(r => r.GetAllAsync(), Times.Never);
        _redisServiceMock.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task GetCities_ShouldFetchFromDatabase_WhenCacheMiss()
    {
        // Arrange
        var cities = new List<City>
        {
            new City { Id = 1, Name = "City1" },
            new City { Id = 2, Name = "City2" }
        };

        var cityDtos = new List<CityDto>
        {
            new CityDto { Id = 1, Name = "City1" },
            new CityDto { Id = 2, Name = "City2" }
        };

        _redisServiceMock
            .Setup(r => r.TryGetAsync<ICollection<CityDto>>(It.IsAny<string>()))
            .ReturnsAsync((false, (ICollection<CityDto>)null!));

        _cityRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(cities);

        _mapperMock
            .Setup(m => m.Map<ICollection<CityDto>>(cities))
            .Returns(cityDtos);

        // Act
        var result = await _service.GetCities();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(cityDtos);

        _cityRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        _mapperMock.Verify(m => m.Map<ICollection<CityDto>>(cities), Times.Once);
    }

    [Fact]
    public async Task GetCities_ShouldCacheResult_WhenCacheMiss()
    {
        // Arrange
        var cities = new List<City>
        {
            new City { Id = 1, Name = "City1" }
        };

        var cityDtos = new List<CityDto>
        {
            new CityDto { Id = 1, Name = "City1" }
        };

        _redisServiceMock
            .Setup(r => r.TryGetAsync<ICollection<CityDto>>(It.IsAny<string>()))
            .ReturnsAsync((false, (ICollection<CityDto>)null!));

        _cityRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(cities);

        _mapperMock
            .Setup(m => m.Map<ICollection<CityDto>>(cities))
            .Returns(cityDtos);

        // Act
        await _service.GetCities();

        // Assert
        _redisServiceMock.Verify(
            r => r.SetAsync(
                "CityDtos",
                It.Is<ICollection<CityDto>>(c => c.Count == 1),
                It.Is<TimeSpan>(t => t.TotalDays == 5)),
            Times.Once);
    }

    [Fact]
    public async Task GetCities_ShouldUseCorrectCacheKey()
    {
        // Arrange
        var cachedCities = new List<CityDto> { new CityDto { Id = 1, Name = "City" } };

        _redisServiceMock
            .Setup(r => r.TryGetAsync<ICollection<CityDto>>("CityDtos"))
            .ReturnsAsync((true, cachedCities));

        // Act
        await _service.GetCities();

        // Assert
        _redisServiceMock.Verify(
            r => r.TryGetAsync<ICollection<CityDto>>("CityDtos"),
            Times.Once);
    }

    [Fact]
    public async Task GetCities_ShouldReturnEmptyCollection_WhenNoCitiesExist()
    {
        // Arrange
        var emptyCities = new List<City>();
        var emptyCityDtos = new List<CityDto>();

        _redisServiceMock
            .Setup(r => r.TryGetAsync<ICollection<CityDto>>(It.IsAny<string>()))
            .ReturnsAsync((false, (ICollection<CityDto>)null!));

        _cityRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(emptyCities);

        _mapperMock
            .Setup(m => m.Map<ICollection<CityDto>>(emptyCities))
            .Returns(emptyCityDtos);

        // Act
        var result = await _service.GetCities();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCities_ShouldUseLongTimeout_WhenCaching()
    {
        // Arrange
        var cities = new List<City> { new City { Id = 1, Name = "City" } };
        var cityDtos = new List<CityDto> { new CityDto { Id = 1, Name = "City" } };

        _redisServiceMock
            .Setup(r => r.TryGetAsync<ICollection<CityDto>>(It.IsAny<string>()))
            .ReturnsAsync((false, (ICollection<CityDto>)null!));

        _cityRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(cities);

        _mapperMock
            .Setup(m => m.Map<ICollection<CityDto>>(cities))
            .Returns(cityDtos);

        // Act
        await _service.GetCities();

        // Assert
        _redisServiceMock.Verify(
            r => r.SetAsync(
                It.IsAny<string>(),
                It.IsAny<ICollection<CityDto>>(),
                It.Is<TimeSpan>(t => t == TimeSpan.FromDays(5))),
            Times.Once);
    }
}
