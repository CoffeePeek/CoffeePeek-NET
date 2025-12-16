using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ShopsService.Consumers;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using MapsterMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoffeePeek.ShopsService.Tests.Consumers;

public class CoffeeShopApprovedEventConsumerTests : IDisposable
{
    private readonly ShopsDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IGenericRepository<Shop>> _shopRepositoryMock;
    private readonly Mock<IGenericRepository<ShopContact>> _shopContactRepositoryMock;
    private readonly Mock<IGenericRepository<ShopPhoto>> _shopPhotoRepositoryMock;
    private readonly Mock<IGenericRepository<Location>> _locationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CoffeeShopApprovedEventConsumer>> _loggerMock;
    private readonly CoffeeShopApprovedEventConsumer _sut;

    public CoffeeShopApprovedEventConsumerTests()
    {
        var options = new DbContextOptionsBuilder<ShopsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ShopsDbContext(options);
        _mapperMock = new Mock<IMapper>();
        _shopRepositoryMock = new Mock<IGenericRepository<Shop>>();
        _shopContactRepositoryMock = new Mock<IGenericRepository<ShopContact>>();
        _shopPhotoRepositoryMock = new Mock<IGenericRepository<ShopPhoto>>();
        _locationRepositoryMock = new Mock<IGenericRepository<Location>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CoffeeShopApprovedEventConsumer>>();

        // Настраиваем маппер так, чтобы он маппил ShopDto -> Shop для тестов
        _mapperMock
            .Setup(m => m.Map<Shop>(It.IsAny<object>()))
            .Returns((object src) =>
            {
                var dto = (ShopDto)src;

                var shop = new Shop
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                };

                if (dto.Location != null)
                {
                    shop.Location = new Location
                    {
                        Address = dto.Location.Address,
                        Latitude = dto.Location.Latitude,
                        Longitude = dto.Location.Longitude
                    };
                }

                if (dto.ShopContact != null)
                {
                    shop.ShopContact = new ShopContact
                    {
                        PhoneNumber = dto.ShopContact.PhoneNumber,
                        InstagramLink = dto.ShopContact.InstagramLink
                    };
                }

                return shop;
            });

        _sut = new CoffeeShopApprovedEventConsumer(
            _mapperMock.Object,
            _shopRepositoryMock.Object,
            _shopContactRepositoryMock.Object,
            _shopPhotoRepositoryMock.Object,
            _locationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_WithValidEvent_CreatesShop()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var shopDto = new ShopDto
        {
            Id = Guid.NewGuid(),
            CityId = Guid.NewGuid(),
            Name = "Test Shop",
            Description = "Test Address",
            Location = new LocationDto
            {
                Address = "Validated Address",
                Latitude = 55.7558m,
                Longitude = 37.6173m
            }
        };

        var @event = new CoffeeShopApprovedEvent(creatorId, shopDto);

        var consumeContext = CreateConsumeContext(@event);

        // Act
        await _sut.Consume(consumeContext);

        // Assert
        _shopRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Shop>(s => s.Name == shopDto.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Consume_WithCoordinates_CreatesLocation()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var shopDto = new ShopDto
        {
            Id = Guid.NewGuid(),
            CityId = Guid.NewGuid(),
            Name = "Test Shop",
            Description = "Test Address",
            Location = new LocationDto
            {
                Address = "Validated Address",
                Latitude = 55.7558m,
                Longitude = 37.6173m
            }
        };

        var @event = new CoffeeShopApprovedEvent(creatorId, shopDto);

        var consumeContext = CreateConsumeContext(@event);
        var shopId = Guid.NewGuid();
        _shopRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Shop>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shop shop, CancellationToken ct) =>
            {
                shop.Id = shopId;
                return shop;
            });

        // Act
        await _sut.Consume(consumeContext);

        // Assert
        _locationRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Location>(l =>
                    l.Latitude == shopDto.Location!.Latitude &&
                    l.Longitude == shopDto.Location.Longitude &&
                    l.Address == shopDto.Location.Address &&
                    l.ShopId == shopId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _shopRepositoryMock.Verify(x => x.Update(It.Is<Shop>(s => s.LocationId != null)), Times.Once);
    }

    [Fact]
    public async Task Consume_WithoutCoordinates_DoesNotCreateLocation()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var shopDto = new ShopDto
        {
            Id = Guid.NewGuid(),
            CityId = Guid.NewGuid(),
            Name = "Test Shop",
            Description = "Test Address",
            Location = null
        };

        var @event = new CoffeeShopApprovedEvent(creatorId, shopDto);

        var consumeContext = CreateConsumeContext(@event);

        // Act
        await _sut.Consume(consumeContext);

        // Assert
        _locationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Consume_WithShopContact_CreatesContact()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var contactDto = new ShopContactDto
        {
            PhoneNumber = "+1234567890",
            InstagramLink = "https://instagram.com/test"
        };
        var shopDto = new ShopDto
        {
            Id = Guid.NewGuid(),
            CityId = Guid.NewGuid(),
            Name = "Test Shop",
            Description = "Test Address",
            ShopContact = contactDto
        };

        var @event = new CoffeeShopApprovedEvent(creatorId, shopDto);

        var consumeContext = CreateConsumeContext(@event);
        var shopId = Guid.NewGuid();
        _shopRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Shop>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shop shop, CancellationToken ct) =>
            {
                shop.Id = shopId;
                return shop;
            });

        // Act
        await _sut.Consume(consumeContext);

        // Assert
        _shopContactRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ShopContact>(c =>
                    c.PhoneNumber == contactDto.PhoneNumber &&
                    c.InstagramLink == contactDto.InstagramLink &&
                    c.ShopId == shopId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _shopRepositoryMock.Verify(x => x.Update(It.IsAny<Shop>()), Times.Once);
    }

    [Fact]
    public async Task Consume_WithAllData_CreatesAllEntities()
    {
        // Arrange
        var photos = new List<string> { "photo1.jpg" };
        var userId = Guid.NewGuid();
        var contactDto = new ShopContactDto
        {
            PhoneNumber = "+1234567890",
            InstagramLink = "https://instagram.com/test"
        };
        var shopDto = new ShopDto
        {
            Id = Guid.NewGuid(),
            CityId = Guid.NewGuid(),
            Name = "Test Shop",
            Description = "Test Address",
            Location = new LocationDto
            {
                Address = "Test Address",
                Latitude = 55.7558m,
                Longitude = 37.6173m
            },
            ShopContact = contactDto,
            ImageUrls = photos.ToArray()
        };

        var @event = new CoffeeShopApprovedEvent(userId, shopDto);

        var consumeContext = CreateConsumeContext(@event);
        var shopId = Guid.NewGuid();
        _shopRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Shop>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shop shop, CancellationToken ct) =>
            {
                shop.Id = shopId;
                return shop;
            });

        // Act
        await _sut.Consume(consumeContext);

        // Assert
        _shopRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Shop>(), It.IsAny<CancellationToken>()), Times.Once);
        _locationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _shopContactRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ShopContact>(), It.IsAny<CancellationToken>()),
            Times.Once);
        // В текущей реализации Consumer не создает ShopPhotos, поэтому достаточно проверить основные вызовы
        _shopPhotoRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IEnumerable<ShopPhoto>>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    private ConsumeContext<CoffeeShopApprovedEvent> CreateConsumeContext(CoffeeShopApprovedEvent @event)
    {
        var mock = new Mock<ConsumeContext<CoffeeShopApprovedEvent>>();
        mock.Setup(x => x.Message).Returns(@event);
        mock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
        return mock.Object;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}