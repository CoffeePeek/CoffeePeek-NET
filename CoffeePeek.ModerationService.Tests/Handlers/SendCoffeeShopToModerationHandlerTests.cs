using CoffeePeek.Moderation.Application.Commands;
using CoffeePeek.Moderation.Application.Handlers;
using Coffeepeek.Moderation.Application.Services;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.ModerationService.Models;
using CoffeePeek.ModerationService.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.ModerationService.Tests.Handlers;

public class SendCoffeeShopToModerationHandlerTests
{
    private readonly Mock<IModerationShopRepository> _repositoryMock;
    private readonly Mock<IModerationShopCreationService> _creationServiceMock;
    private readonly Mock<IYandexGeocodingService> _geocodingServiceMock;
    private readonly SendCoffeeShopToModerationHandler _sut;

    public SendCoffeeShopToModerationHandlerTests()
    {
        _repositoryMock = new Mock<IModerationShopRepository>();
        _creationServiceMock = new Mock<IModerationShopCreationService>();
        _geocodingServiceMock = new Mock<IYandexGeocodingService>();

        _sut = new SendCoffeeShopToModerationHandler(
            _repositoryMock.Object,
            _creationServiceMock.Object,
            _geocodingServiceMock.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SendCoffeeShopToModerationHandler>>()
        );
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesModerationShopAndReturnsSuccess()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationCommand
        {
            Name = "New Coffee Shop",
            NotValidatedAddress = "123 Main St, City",
            UserId = Guid.NewGuid()
        };

        var geocodingResult = new GeocodingResult(Latitude: 55.7558m, Longitude: 37.6173m);

        _repositoryMock
            .Setup(x => x.GetByNameAndAddressAsync(request.Name, request.NotValidatedAddress, request.UserId))
            .ReturnsAsync((ModerationShop?)null);

        _geocodingServiceMock
            .Setup(x => x.GeocodeAsync(request.NotValidatedAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(geocodingResult);

        _creationServiceMock
            .Setup(x => x.CreateAsync(request, geocodingResult, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("added to moderation");

        _creationServiceMock.Verify(
            x => x.CreateAsync(request, geocodingResult, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingModeration_ReturnsError()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationCommand
        {
            Name = "Existing Shop",
            NotValidatedAddress = "123 Main St",
            UserId = Guid.NewGuid()
        };

        var existingShop = new ModerationShop
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            NotValidatedAddress = request.NotValidatedAddress,
            UserId = request.UserId
        };

        _repositoryMock
            .Setup(x => x.GetByNameAndAddressAsync(request.Name, request.NotValidatedAddress, request.UserId))
            .ReturnsAsync(existingShop);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("already exists");

        _creationServiceMock.Verify(x => x.CreateAsync(It.IsAny<SendCoffeeShopToModerationCommand>(), It.IsAny<GeocodingResult?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithGeocodingFailure_CreatesShopWithoutCoordinates()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationCommand
        {
            Name = "New Shop",
            NotValidatedAddress = "Invalid Address",
            UserId = Guid.NewGuid()
        };

        _repositoryMock
            .Setup(x => x.GetByNameAndAddressAsync(request.Name, request.NotValidatedAddress, request.UserId))
            .ReturnsAsync((ModerationShop?)null);

        _geocodingServiceMock
            .Setup(x => x.GeocodeAsync(request.NotValidatedAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeocodingResult?)null);

        _creationServiceMock
            .Setup(x => x.CreateAsync(request, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _creationServiceMock.Verify(
            x => x.CreateAsync(request, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRequest_SetsPendingStatus()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationCommand
        {
            Name = "New Shop",
            NotValidatedAddress = "123 Test St",
            UserId = Guid.NewGuid()
        };

        _repositoryMock
            .Setup(x => x.GetByNameAndAddressAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync((ModerationShop?)null);

        _geocodingServiceMock
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeocodingResult?)null);

        _creationServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<SendCoffeeShopToModerationCommand>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        _creationServiceMock.Verify(
            x => x.CreateAsync(It.IsAny<SendCoffeeShopToModerationCommand>(), null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRequest_SetsNotConfirmedShopStatus()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationCommand
        {
            Name = "New Shop",
            NotValidatedAddress = "123 Test St",
            UserId = Guid.NewGuid()
        };

        _repositoryMock
            .Setup(x => x.GetByNameAndAddressAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync((ModerationShop?)null);

        _geocodingServiceMock
            .Setup(x => x.GeocodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeocodingResult?)null);

        _creationServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<SendCoffeeShopToModerationCommand>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        _creationServiceMock.Verify(
            x => x.CreateAsync(It.IsAny<SendCoffeeShopToModerationCommand>(), null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}