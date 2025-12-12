using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Handlers;
using CoffeePeek.ModerationService.Models;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using CoffeePeek.ModerationService.Services.Interfaces;
using CoffeePeek.ModerationService.Services.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.ModerationService.Tests.Handlers;

public class SendCoffeeShopToModerationHandlerTests
{
    private readonly Mock<IModerationShopRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IYandexGeocodingService> _geocodingServiceMock;
    private readonly SendCoffeeShopToModerationHandler _sut;

    public SendCoffeeShopToModerationHandlerTests()
    {
        _repositoryMock = new Mock<IModerationShopRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _geocodingServiceMock = new Mock<IYandexGeocodingService>();

        _sut = new SendCoffeeShopToModerationHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _geocodingServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesModerationShopAndReturnsSuccess()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationRequest
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

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("added to moderation");

        _repositoryMock.Verify(
            x => x.AddAsync(It.Is<ModerationShop>(s =>
                s.Name == request.Name &&
                s.NotValidatedAddress == request.NotValidatedAddress &&
                s.UserId == request.UserId &&
                s.IsAddressValidated == true &&
                s.Latitude == geocodingResult.Latitude &&
                s.Longitude == geocodingResult.Longitude
            )),
            Times.Once
        );

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingModeration_ReturnsError()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationRequest
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

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<ModerationShop>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithGeocodingFailure_CreatesShopWithoutCoordinates()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationRequest
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

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Verify(
            x => x.AddAsync(It.Is<ModerationShop>(s =>
                s.IsAddressValidated == false &&
                s.Latitude == null &&
                s.Longitude == null
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithValidRequest_SetsPendingStatus()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationRequest
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

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.AddAsync(It.Is<ModerationShop>(s =>
                s.ModerationStatus == Contract.Enums.ModerationStatus.Pending
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithValidRequest_SetsNotConfirmedShopStatus()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationRequest
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

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.AddAsync(It.Is<ModerationShop>(s =>
                s.Status == Contract.Enums.ShopStatus.NotConfirmed
            )),
            Times.Once
        );
    }
}