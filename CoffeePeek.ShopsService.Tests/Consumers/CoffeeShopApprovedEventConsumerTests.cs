using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ShopsService.Consumers;
using CoffeePeek.ShopsService.DB;
using CoffeePeek.ShopsService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CoffeePeek.ShopsService.Tests.Consumers;

public class CoffeeShopApprovedEventConsumerTests : IDisposable
{
    private readonly ShopsDbContext _dbContext;
    private readonly Mock<IGenericRepository<Shop>> _shopRepositoryMock;
    private readonly Mock<IGenericRepository<ShopContact>> _shopContactRepositoryMock;
    private readonly Mock<IGenericRepository<ShopPhoto>> _shopPhotoRepositoryMock;
    private readonly Mock<IGenericRepository<Location>> _locationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CoffeeShopApprovedEventConsumer _sut;

    public CoffeeShopApprovedEventConsumerTests()
    {
        var options = new DbContextOptionsBuilder<ShopsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ShopsDbContext(options);
        _shopRepositoryMock = new Mock<IGenericRepository<Shop>>();
        _shopContactRepositoryMock = new Mock<IGenericRepository<ShopContact>>();
        _shopPhotoRepositoryMock = new Mock<IGenericRepository<ShopPhoto>>();
        _locationRepositoryMock = new Mock<IGenericRepository<Location>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new CoffeeShopApprovedEventConsumer(
            _shopRepositoryMock.Object,
            _shopContactRepositoryMock.Object,
            _shopPhotoRepositoryMock.Object,
            _locationRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Consume_WithValidEvent_CreatesShop()
    {
        // Arrange
        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Shop",
            NotValidatedAddress: "Test Address",
            UserId: Guid.NewGuid(),
            address: "Validated Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: new List<string>(),
            Schedules: new List<ScheduleDto>(),
            Latitude: 55.7558m,
            Longitude: 37.6173m
        );

        var consumeContext = CreateConsumeContext(@event);

        // Act
        await _sut.Consume(consumeContext);

        // Assert
        _shopRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Shop>(s => s.Name == @event.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Consume_WithCoordinates_CreatesLocation()
    {
        // Arrange
        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Shop",
            NotValidatedAddress: "Test Address",
            UserId: Guid.NewGuid(),
            address: "Validated Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: new List<string>(),
            Schedules: new List<ScheduleDto>(),
            Latitude: 55.7558m,
            Longitude: 37.6173m
        );

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
                    l.Latitude == @event.Latitude &&
                    l.Longitude == @event.Longitude &&
                    l.Address == @event.address &&
                    l.ShopId == shopId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _shopRepositoryMock.Verify(x => x.Update(It.Is<Shop>(s => s.LocationId != null)), Times.Once);
    }

    [Fact]
    public async Task Consume_WithoutCoordinates_DoesNotCreateLocation()
    {
        // Arrange
        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Shop",
            NotValidatedAddress: "Test Address",
            UserId: Guid.NewGuid(),
            address: "Validated Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: new List<string>(),
            Schedules: new List<ScheduleDto>(),
            Latitude: null,
            Longitude: null
        );

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
        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Shop",
            NotValidatedAddress: "Test Address",
            UserId: Guid.NewGuid(),
            address: "Validated Address",
            ShopContactId: Guid.NewGuid(),
            Status: ShopStatus.NotConfirmed,
            ShopContact: new ShopContactDto
            {
                PhoneNumber = "+1234567890",
                InstagramLink = "https://instagram.com/test"
            },
            ShopPhotos: new List<string>(),
            Schedules: new List<ScheduleDto>(),
            Latitude: 55.7558m,
            Longitude: 37.6173m
        );

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
                    c.PhoneNumber == @event.ShopContact!.PhoneNumber &&
                    c.InstagramLink == @event.ShopContact.InstagramLink &&
                    c.ShopId == shopId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        // Update is called twice: once for LocationId (when coordinates are present), once for ShopContactId
        // Since the shop object is mutable, we just verify the total count
        _shopRepositoryMock.Verify(x => x.Update(It.IsAny<Shop>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Consume_WithShopPhotos_CreatesPhotos()
    {
        // Arrange
        var photos = new List<string> { "photo1.jpg", "photo2.jpg", "photo3.jpg" };
        var userId = Guid.NewGuid();
        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Shop",
            NotValidatedAddress: "Test Address",
            UserId: userId,
            address: "Validated Address",
            ShopContactId: null,
            Status: ShopStatus.NotConfirmed,
            ShopContact: null,
            ShopPhotos: photos,
            Schedules: new List<ScheduleDto>(),
            Latitude: 55.7558m,
            Longitude: 37.6173m
        );

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
        _shopPhotoRepositoryMock.Verify(
            x => x.AddRangeAsync(
                It.Is<IEnumerable<ShopPhoto>>(p =>
                    p.Count() == photos.Count &&
                    p.All(photo => photo.ShopId == shopId && photo.UserId == userId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_WithAllData_CreatesAllEntities()
    {
        // Arrange
        var photos = new List<string> { "photo1.jpg" };
        var userId = Guid.NewGuid();
        var @event = new CoffeeShopApprovedEvent(
            ModerationShopId: Guid.NewGuid(),
            Name: "Test Shop",
            NotValidatedAddress: "Test Address",
            UserId: userId,
            address: "Validated Address",
            ShopContactId: Guid.NewGuid(),
            Status: ShopStatus.NotConfirmed,
            ShopContact: new ShopContactDto
            {
                PhoneNumber = "+1234567890",
                InstagramLink = "https://instagram.com/test"
            },
            ShopPhotos: photos,
            Schedules: new List<ScheduleDto>(),
            Latitude: 55.7558m,
            Longitude: 37.6173m
        );

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
        _shopPhotoRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IEnumerable<ShopPhoto>>(), It.IsAny<CancellationToken>()), Times.Once);
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