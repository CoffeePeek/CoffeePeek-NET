using CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop;
using CoffeePeek.Contract.Dtos.Address;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Photos;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;
using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.CoffeeShop;

public class GetCoffeeShopsRequestHandlerTests
{
    private readonly Mock<IUnitOfWork<CoffeePeekDbContext>> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<DbSet<Shop>> _shopsDbSetMock;
    private readonly GetCoffeeShopsRequestHandler _handler;

    public GetCoffeeShopsRequestHandlerTests()
    {
        // Setup mocks
        _unitOfWorkMock = new Mock<IUnitOfWork<CoffeePeekDbContext>>();
        _mapperMock = new Mock<IMapper>();
        _shopsDbSetMock = new Mock<DbSet<Shop>>();

        // Setup DbContext mock
        var dbContextMock = new Mock<CoffeePeekDbContext>();
        dbContextMock.Setup(db => db.Shops).Returns(_shopsDbSetMock.Object);
        _unitOfWorkMock.Setup(uow => uow.DbContext).Returns(dbContextMock.Object);

        // Create handler instance
        _handler = new GetCoffeeShopsRequestHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCoffeeShops_WhenShopsExist()
    {
        // Arrange
        var request = new GetCoffeeShopsRequest(cityId: 1, pageNumber: 1, pageSize: 10);
        
        var shops = new List<Shop>
        {
            new Shop
            {
                Id = 1,
                Name = "Coffee Shop 1",
                AddressId = 1,
                Address = new Address
                {
                    Id = 1,
                    City = new City { Id = 1, Name = "City 1" },
                    Street = new Street { Id = 1, Name = "Street 1" }
                }
            },
            new Shop
            {
                Id = 2,
                Name = "Coffee Shop 2",
                AddressId = 2,
                Address = new Address
                {
                    Id = 2,
                    City = new City { Id = 1, Name = "City 1" },
                    Street = new Street { Id = 2, Name = "Street 2" }
                }
            }
        };

        var coffeeShopDtos = new CoffeeShopDto[]
        {
            new CoffeeShopDto
            {
                Id = 1,
                Name = "Coffee Shop 1",
                Address = new AddressDto(),
                ShopContact = new ShopContactDto(),
                ShopPhotos = new List<ShopPhotoDtos>(),
                Schedules = new List<ScheduleDto>()
            },
            new CoffeeShopDto
            {
                Id = 2,
                Name = "Coffee Shop 2",
                Address = new AddressDto(),
                ShopContact = new ShopContactDto(),
                ShopPhotos = new List<ShopPhotoDtos>(),
                Schedules = new List<ScheduleDto>()
            }
        };

        // Setup mock for Count
        _shopsDbSetMock.Setup(s => s.Count()).Returns(shops.Count);

        // Setup mock for ToListAsync
        var shopsQueryable = shops.AsQueryable();
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.Provider).Returns(shopsQueryable.Provider);
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.Expression).Returns(shopsQueryable.Expression);
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.ElementType).Returns(shopsQueryable.ElementType);
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.GetEnumerator()).Returns(shopsQueryable.GetEnumerator());

        _mapperMock
            .Setup(m => m.Map<CoffeeShopDto[]>(It.IsAny<IEnumerable<Shop>>()))
            .Returns(coffeeShopDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CoffeeShopDtos.Should().HaveCount(2);
        result.Data.CurrentPage.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
        result.Data.TotalItems.Should().Be(2);
        result.Data.TotalPages.Should().Be(1);

        _unitOfWorkMock.Verify(
            uow => uow.DbContext.Shops.Count(),
            Times.Once);

        _mapperMock.Verify(
            m => m.Map<CoffeeShopDto[]>(It.IsAny<IEnumerable<Shop>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoShopsExist()
    {
        // Arrange
        var request = new GetCoffeeShopsRequest(cityId: 1, pageNumber: 1, pageSize: 10);
        
        var shops = new List<Shop>();

        // Setup mock for Count
        _shopsDbSetMock.Setup(s => s.Count()).Returns(shops.Count);

        // Setup mock for ToListAsync
        var shopsQueryable = shops.AsQueryable();
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.Provider).Returns(shopsQueryable.Provider);
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.Expression).Returns(shopsQueryable.Expression);
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.ElementType).Returns(shopsQueryable.ElementType);
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.GetEnumerator()).Returns(shopsQueryable.GetEnumerator());

        _mapperMock
            .Setup(m => m.Map<CoffeeShopDto[]>(It.IsAny<IEnumerable<Shop>>()))
            .Returns(Array.Empty<CoffeeShopDto>());

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CoffeeShopDtos.Should().BeEmpty();
        result.Data.CurrentPage.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
        result.Data.TotalItems.Should().Be(0);
        result.Data.TotalPages.Should().Be(0);

        _unitOfWorkMock.Verify(
            uow => uow.DbContext.Shops.Count(),
            Times.Once);

        _mapperMock.Verify(
            m => m.Map<CoffeeShopDto[]>(It.IsAny<IEnumerable<Shop>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAdjustPageNumber_WhenRequestedPageExceedsTotalPages()
    {
        // Arrange
        var request = new GetCoffeeShopsRequest(cityId: 1, pageNumber: 5, pageSize: 10);
        
        var shops = new List<Shop>
        {
            new Shop { Id = 1, Name = "Coffee Shop 1", AddressId = 1 }
        };

        var coffeeShopDtos = new CoffeeShopDto[]
        {
            new CoffeeShopDto
            {
                Id = 1,
                Name = "Coffee Shop 1",
                Address = new AddressDto(),
                ShopContact = new ShopContactDto(),
                ShopPhotos = new List<ShopPhotoDtos>(),
                Schedules = new List<ScheduleDto>()
            }
        };

        // Setup mock for Count - only 1 shop, so with pageSize=10, totalPages=1
        _shopsDbSetMock.Setup(s => s.Count()).Returns(shops.Count);

        // Setup mock for ToListAsync
        var shopsQueryable = shops.AsQueryable();
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.Provider).Returns(shopsQueryable.Provider);
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.Expression).Returns(shopsQueryable.Expression);
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.ElementType).Returns(shopsQueryable.ElementType);
        _shopsDbSetMock.As<IQueryable<Shop>>().Setup(m => m.GetEnumerator()).Returns(shopsQueryable.GetEnumerator());

        _mapperMock
            .Setup(m => m.Map<CoffeeShopDto[]>(It.IsAny<IEnumerable<Shop>>()))
            .Returns(coffeeShopDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        // Should return page 1 (adjusted from page 5) since there's only 1 page
        result.Data!.CurrentPage.Should().Be(1);
        result.Data.TotalPages.Should().Be(1);

        _unitOfWorkMock.Verify(
            uow => uow.DbContext.Shops.Count(),
            Times.Once);
    }
}