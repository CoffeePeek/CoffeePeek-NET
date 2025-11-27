using CoffeePeek.BusinessLogic.RequestHandlers.Internal;
using CoffeePeek.Contract.Dtos.Address;
using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Response;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using MapsterMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.Internal;

public class GetCitiesRequestHandlerTests
{
    private readonly Mock<CoffeePeekDbContext> _dbContextMock;
    private readonly Mock<DbSet<City>> _citiesDbSetMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCitiesRequestHandler _handler;

    public GetCitiesRequestHandlerTests()
    {
        // Setup mocks
        _dbContextMock = new Mock<CoffeePeekDbContext>();
        _citiesDbSetMock = new Mock<DbSet<City>>();
        _mapperMock = new Mock<IMapper>();
        var mockCacheService = new Mock<ICacheService>();

        // Setup DbContext mock
        _dbContextMock.Setup(db => db.Cities).Returns(_citiesDbSetMock.Object);

        // Create handler instance
        _handler = new GetCitiesRequestHandler(mockCacheService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllCities()
    {
        // Arrange
        var request = new GetCitiesRequest();
        var cities = new List<City>
        {
            new() { Id = 1, Name = "City 1" },
            new() { Id = 2, Name = "City 2" }
        };
        
        var cityDtos = new CityDto[]
        {
            new() { Id = 1, Name = "City 1" },
            new() { Id = 2, Name = "City 2" }
        };

        _citiesDbSetMock
            .Setup(c => c.AsNoTracking())
            .Returns(_citiesDbSetMock.Object);

        _citiesDbSetMock
            .Setup(c => c.ToArrayAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities.ToArray());
            
        _mapperMock
            .Setup(m => m.Map<CityDto[]>(It.IsAny<IEnumerable<City>>()))
            .Returns(cityDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Cities.Should().HaveCount(2);

        _citiesDbSetMock.Verify(c => c.AsNoTracking(), Times.Once);
        _citiesDbSetMock.Verify(c => c.ToArrayAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<CityDto[]>(It.IsAny<IEnumerable<City>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyArray_WhenNoCitiesExist()
    {
        // Arrange
        var request = new GetCitiesRequest();
        var cities = new List<City>();
        var cityDtos = Array.Empty<CityDto>();

        _citiesDbSetMock
            .Setup(c => c.AsNoTracking())
            .Returns(_citiesDbSetMock.Object);

        _citiesDbSetMock
            .Setup(c => c.ToArrayAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities.ToArray());
            
        _mapperMock
            .Setup(m => m.Map<CityDto[]>(It.IsAny<IEnumerable<City>>()))
            .Returns(cityDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Cities.Should().BeEmpty();

        _citiesDbSetMock.Verify(c => c.AsNoTracking(), Times.Once);
        _citiesDbSetMock.Verify(c => c.ToArrayAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<CityDto[]>(It.IsAny<IEnumerable<City>>()), Times.Once);
    }
}