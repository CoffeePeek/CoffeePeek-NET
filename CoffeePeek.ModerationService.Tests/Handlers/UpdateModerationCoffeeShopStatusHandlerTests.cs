using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Handlers;
using CoffeePeek.ModerationService.Models;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using FluentAssertions;
using MassTransit;
using Moq;
using Xunit;

namespace CoffeePeek.ModerationService.Tests.Handlers;

public class UpdateModerationCoffeeShopStatusHandlerTests
{
    private readonly Mock<IModerationShopRepository> _repositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateModerationCoffeeShopStatusHandler _sut;

    public UpdateModerationCoffeeShopStatusHandlerTests()
    {
        _repositoryMock = new Mock<IModerationShopRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new UpdateModerationCoffeeShopStatusHandler(
            _repositoryMock.Object,
            _publishEndpointMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenShopNotFound_ReturnsError()
    {
        // Arrange
        var request = new UpdateModerationCoffeeShopStatusRequest(
            id: Guid.NewGuid(),
            moderationStatus: ModerationStatus.Approved,
            userId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(request.Id))
            .ReturnsAsync((ModerationShop?)null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("CoffeeShop not found");
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()),
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
            Status = ShopStatus.NotConfirmed,
            Latitude = 55.7558m,
            Longitude = 37.6173m,
            ShopPhotos = new List<ShopPhoto>(),
            Schedules = new List<Schedule>()
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            id: shopId,
            moderationStatus: ModerationStatus.Approved,
            userId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<CoffeeShopApprovedEvent>(e =>
                    e.ModerationShopId == shopId &&
                    e.Name == shop.Name &&
                    e.Latitude == shop.Latitude &&
                    e.Longitude == shop.Longitude),
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
            Status = ShopStatus.NotConfirmed,
            Latitude = latitude,
            Longitude = longitude,
            ShopPhotos = new List<ShopPhoto>(),
            Schedules = new List<Schedule>()
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            id: shopId,
            moderationStatus: ModerationStatus.Approved,
            userId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        CoffeeShopApprovedEvent? publishedEvent = null;
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CoffeeShopApprovedEvent, CancellationToken>((e, ct) => publishedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        publishedEvent.Should().NotBeNull();
        publishedEvent!.Latitude.Should().Be(latitude);
        publishedEvent.Longitude.Should().Be(longitude);
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
            Status = ShopStatus.NotConfirmed,
            ShopContacts = new ShopContacts
            {
                PhoneNumber = "+1234567890",
                InstagramLink = "https://instagram.com/test"
            },
            ShopPhotos = new List<ShopPhoto>(),
            Schedules = new List<Schedule>()
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            id: shopId,
            moderationStatus: ModerationStatus.Approved,
            userId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        CoffeeShopApprovedEvent? publishedEvent = null;
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CoffeeShopApprovedEvent, CancellationToken>((e, ct) => publishedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        publishedEvent.Should().NotBeNull();
        publishedEvent!.ShopContact.Should().NotBeNull();
        publishedEvent.ShopContact!.PhoneNumber.Should().Be("+1234567890");
        publishedEvent.ShopContact.InstagramLink.Should().Be("https://instagram.com/test");
    }

    [Fact]
    public async Task Handle_WhenApproved_IncludesPhotosInEvent()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var photos = new List<ShopPhoto>
        {
            new ShopPhoto { Url = "photo1.jpg" },
            new ShopPhoto { Url = "photo2.jpg" }
        };

        var shop = new ModerationShop
        {
            Id = shopId,
            Name = "Test Shop",
            NotValidatedAddress = "Test Address",
            Address = "Validated Address",
            UserId = userId,
            ModerationStatus = ModerationStatus.Pending,
            Status = ShopStatus.NotConfirmed,
            ShopPhotos = photos,
            Schedules = new List<Schedule>()
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            id: shopId,
            moderationStatus: ModerationStatus.Approved,
            userId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        CoffeeShopApprovedEvent? publishedEvent = null;
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CoffeeShopApprovedEvent, CancellationToken>((e, ct) => publishedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        publishedEvent.Should().NotBeNull();
        publishedEvent!.ShopPhotos.Should().HaveCount(2);
        publishedEvent.ShopPhotos.Should().Contain("photo1.jpg");
        publishedEvent.ShopPhotos.Should().Contain("photo2.jpg");
    }

    [Fact]
    public async Task Handle_WhenApproved_IncludesSchedulesInEvent()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var schedules = new List<Schedule>
        {
            new Schedule
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
            Status = ShopStatus.NotConfirmed,
            ShopPhotos = new List<ShopPhoto>(),
            Schedules = schedules
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            id: shopId,
            moderationStatus: ModerationStatus.Approved,
            userId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        CoffeeShopApprovedEvent? publishedEvent = null;
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CoffeeShopApprovedEvent, CancellationToken>((e, ct) => publishedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        publishedEvent.Should().NotBeNull();
        publishedEvent!.Schedules.Should().HaveCount(1);
        publishedEvent.Schedules.First().DayOfWeek.Should().Be(DayOfWeek.Monday);
        publishedEvent.Schedules.First().OpeningTime.Should().Be(TimeSpan.FromHours(9));
        publishedEvent.Schedules.First().ClosingTime.Should().Be(TimeSpan.FromHours(18));
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
            Status = ShopStatus.NotConfirmed,
            ShopPhotos = new List<ShopPhoto>(),
            Schedules = new List<Schedule>()
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            id: shopId,
            moderationStatus: ModerationStatus.Pending,
            userId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()),
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
            Status = ShopStatus.NotConfirmed,
            ShopPhotos = new List<ShopPhoto>(),
            Schedules = new List<Schedule>()
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            id: shopId,
            moderationStatus: ModerationStatus.Pending,
            userId: Guid.NewGuid()
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(shopId))
            .ReturnsAsync(shop);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<CoffeeShopApprovedEvent>(), It.IsAny<CancellationToken>()),
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
            Status = ShopStatus.NotConfirmed,
            ShopPhotos = new List<ShopPhoto>(),
            Schedules = new List<Schedule>()
        };

        var request = new UpdateModerationCoffeeShopStatusRequest(
            id: shopId,
            moderationStatus: ModerationStatus.Approved,
            userId: Guid.NewGuid()
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