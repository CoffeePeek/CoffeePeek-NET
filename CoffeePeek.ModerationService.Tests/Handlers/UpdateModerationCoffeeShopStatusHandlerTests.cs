using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Entities;
using CoffeePeek.ModerationService.Handlers;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Outbox;
using FluentAssertions;
using MapsterMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoffeePeek.ModerationService.Tests.Handlers;

public class UpdateModerationCoffeeShopStatusHandlerTests
{
    private readonly Mock<IModerationShopRepository> _repositoryMock;
    private readonly Mock<IOutboxEventPublisher> _publishEndpointMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateModerationCoffeeShopStatusHandler _sut;

    public UpdateModerationCoffeeShopStatusHandlerTests()
    {
        _repositoryMock = new Mock<IModerationShopRepository>();
        _publishEndpointMock = new Mock<IOutboxEventPublisher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<UpdateModerationCoffeeShopStatusHandler>>();
        var publishEndpoint = new Mock<IPublishEndpoint>();

        _sut = new UpdateModerationCoffeeShopStatusHandler(
            _repositoryMock.Object,
            _publishEndpointMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            loggerMock.Object,
            publishEndpoint.Object
        );
    }

    [Fact]
    public async Task Handle_WhenShopNotFound_ReturnsError()
    {
        // Arrange
        var request = new UpdateModerationCoffeeShopStatusRequest(
            Id: Guid.NewGuid(),
            ModerationStatus: ModerationStatus.Approved,
            UserId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(request.Id))
            .ReturnsAsync((ModerationShop?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.Handle(request, CancellationToken.None));

        _publishEndpointMock.Verify(
            x => x.PublishAsync(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenApproved_PublishesEvent()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shop = new ModerationShop
        {
            Id = shopId,
            Name = "Test Shop",
            NotValidatedAddress = "Test Address",
            Address = "Validated Address",
            UserId = userId,
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            Id: shopId,
            ModerationStatus: ModerationStatus.Approved,
            UserId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        var mappedShop = new ShopDto
        {
            Id = shopId,
            Name = shop.Name
        };

        _mapperMock
            .Setup(m => m.Map<ShopDto>(shop))
            .Returns(mappedShop);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _publishEndpointMock.Verify(
            x => x.PublishAsync(
                It.Is<CoffeeShopApprovedEvent>(e =>
                    e.CreatorId == request.UserId &&
                    e.Shop == mappedShop),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(shop), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenApproved_IncludesCoordinatesInEvent()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var latitude = 55.7558m;
        var longitude = 37.6173m;

        var shop = new ModerationShop
        {
            Id = shopId,
            Name = "Test Shop",
            NotValidatedAddress = "Test Address",
            Address = "Validated Address",
            UserId = userId,
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            Id: shopId,
            ModerationStatus: ModerationStatus.Approved,
            UserId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        var mappedShop = new ShopDto
        {
            Id = shopId,
            Name = shop.Name,
            Location = new CoffeePeek.Contract.Dtos.Shop.LocationDto
            {
                Address = shop.Address,
                Latitude = latitude,
                Longitude = longitude
            }
        };

        _mapperMock
            .Setup(m => m.Map<ShopDto>(shop))
            .Returns(mappedShop);

        CoffeeShopApprovedEvent? publishedEvent = null;
        _publishEndpointMock
            .Setup(x => x.PublishAsync(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CoffeeShopApprovedEvent, CancellationToken>((e, ct) => publishedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        publishedEvent.Should().NotBeNull();
        publishedEvent!.Shop.Location.Should().NotBeNull();
        publishedEvent.Shop.Location!.Latitude.Should().Be(latitude);
        publishedEvent.Shop.Location.Longitude.Should().Be(longitude);
    }

    [Fact]
    public async Task Handle_WhenApproved_IncludesContactInEvent()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        var shop = new ModerationShop
        {
            Id = shopId,
            Name = "Test Shop",
            NotValidatedAddress = "Test Address",
            Address = "Validated Address",
            UserId = userId,
            ShopContactId = contactId,
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            Id: shopId,
            ModerationStatus: ModerationStatus.Approved,
            UserId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        var mappedShop = new ShopDto
        {
            Id = shopId,
            Name = shop.Name,
            ShopContact = new ShopContactDto
            {
                PhoneNumber = "+1234567890",
                InstagramLink = "https://instagram.com/test"
            }
        };

        _mapperMock
            .Setup(m => m.Map<ShopDto>(shop))
            .Returns(mappedShop);

        CoffeeShopApprovedEvent? publishedEvent = null;
        _publishEndpointMock
            .Setup(x => x.PublishAsync(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CoffeeShopApprovedEvent, CancellationToken>((e, ct) => publishedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        publishedEvent.Should().NotBeNull();
        publishedEvent!.Shop.ShopContact.Should().NotBeNull();
        publishedEvent.Shop.ShopContact!.PhoneNumber.Should().Be("+1234567890");
        publishedEvent.Shop.ShopContact.InstagramLink.Should().Be("https://instagram.com/test");
    }

    [Fact]
    public async Task Handle_WhenApproved_IncludesPhotosInEvent()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var photos = new List<string> { "photo1.jpg", "photo2.jpg" };

        var shop = new ModerationShop
        {
            Id = shopId,
            Name = "Test Shop",
            NotValidatedAddress = "Test Address",
            Address = "Validated Address",
            UserId = userId,
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            Id: shopId,
            ModerationStatus: ModerationStatus.Approved,
            UserId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        var mappedShop = new ShopDto
        {
            Id = shopId,
            Name = shop.Name,
            ImageUrls = photos.ToArray()
        };

        _mapperMock
            .Setup(m => m.Map<ShopDto>(shop))
            .Returns(mappedShop);

        CoffeeShopApprovedEvent? publishedEvent = null;
        _publishEndpointMock
            .Setup(x => x.PublishAsync(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CoffeeShopApprovedEvent, CancellationToken>((e, ct) => publishedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        publishedEvent.Should().NotBeNull();
        publishedEvent!.Shop.ImageUrls.Should().NotBeNull();
        publishedEvent.Shop.ImageUrls!.Should().HaveCount(2);
        publishedEvent.Shop.ImageUrls.Should().Contain("photo1.jpg");
        publishedEvent.Shop.ImageUrls.Should().Contain("photo2.jpg");
    }

    [Fact]
    public async Task Handle_WhenApproved_IncludesSchedulesInEvent()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var schedules = new List<ScheduleDto>
        {
            new ScheduleDto
            {
                DayOfWeek = DayOfWeek.Monday,
                OpeningTime = TimeSpan.FromHours(9),
                ClosingTime = TimeSpan.FromHours(18)
            }
        };

        var shop = new ModerationShop
        {
            Id = shopId,
            Name = "Test Shop",
            NotValidatedAddress = "Test Address",
            Address = "Validated Address",
            UserId = userId,
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            Id: shopId,
            ModerationStatus: ModerationStatus.Approved,
            UserId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        var mappedShop = new ShopDto
        {
            Id = shopId,
            Name = shop.Name,
            Schedules = schedules
        };

        _mapperMock
            .Setup(m => m.Map<ShopDto>(shop))
            .Returns(mappedShop);

        CoffeeShopApprovedEvent? publishedEvent = null;
        _publishEndpointMock
            .Setup(x => x.PublishAsync(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CoffeeShopApprovedEvent, CancellationToken>((e, ct) => publishedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        publishedEvent.Should().NotBeNull();
        publishedEvent!.Shop.Schedules.Should().NotBeNull();
        publishedEvent.Shop.Schedules!.Should().HaveCount(1);
        publishedEvent.Shop.Schedules.First().DayOfWeek.Should().Be(DayOfWeek.Monday);
        publishedEvent.Shop.Schedules.First().OpeningTime.Should().Be(TimeSpan.FromHours(9));
        publishedEvent.Shop.Schedules.First().ClosingTime.Should().Be(TimeSpan.FromHours(18));
    }

    [Fact]
    public async Task Handle_WhenNotApproved_DoesNotPublishEvent()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var shop = new ModerationShop
        {
            Id = shopId,
            Name = "Test Shop",
            NotValidatedAddress = "Test Address",
            Address = "Validated Address",
            UserId = Guid.NewGuid(),
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            Id: shopId,
            ModerationStatus: ModerationStatus.Pending,
            UserId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _publishEndpointMock.Verify(
            x => x.PublishAsync(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repositoryMock.Verify(x => x.UpdateAsync(shop), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPending_DoesNotPublishEvent()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var shop = new ModerationShop
        {
            Id = shopId,
            Name = "Test Shop",
            NotValidatedAddress = "Test Address",
            Address = "Validated Address",
            UserId = Guid.NewGuid(),
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            Id: shopId,
            ModerationStatus: ModerationStatus.Pending,
            UserId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _publishEndpointMock.Verify(
            x => x.PublishAsync(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatesModerationStatus()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var shop = new ModerationShop
        {
            Id = shopId,
            Name = "Test Shop",
            NotValidatedAddress = "Test Address",
            Address = "Validated Address",
            UserId = Guid.NewGuid(),
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            Id: shopId,
            ModerationStatus: ModerationStatus.Approved,
            UserId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        shop.ModerationStatus.Should().Be(ModerationStatus.Approved);
        _repositoryMock.Verify(x => x.UpdateAsync(shop), Times.Once);
    }
}