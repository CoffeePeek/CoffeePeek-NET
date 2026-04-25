using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Shops;
using CoffeePeek.Contract.Dtos.Internal;
using FluentAssertions;
using FluentResults;
using Moq;
using Xunit;

namespace CoffeePeek.Client.App.Tests.ViewModels.Shops;

public class SuggestShopViewModelTests
{
    private readonly Mock<IWebCatalogsClient> _catalogsClientMock = new();
    private readonly Mock<IWebModerationShopsClient> _moderationClientMock = new();
    private readonly Mock<IImagePickerService> _imagePickerMock = new();
    private readonly Mock<IWorkspaceShellNavigator> _navigatorMock = new();

    private SuggestShopViewModel CreateSut()
    {
        _catalogsClientMock
            .Setup(c => c.GetCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetCitiesResultDto
            {
                Cities = [new CityDto { Id = Guid.NewGuid(), Name = "Minsk" }]
            }));
        _catalogsClientMock
            .Setup(c => c.GetBeansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetBeansResultDto()));
        _catalogsClientMock
            .Setup(c => c.GetRoastersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetRoastersResultDto()));
        _catalogsClientMock
            .Setup(c => c.GetEquipmentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetEquipmentResultDto()));
        _catalogsClientMock
            .Setup(c => c.GetBrewMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new GetBrewMethodsResultDto()));

        _moderationClientMock
            .Setup(c => c.SendSuggestionAsync(It.IsAny<SendCoffeeShopToModerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new SendCoffeeShopToModerationResponseDto { ShopId = Guid.NewGuid(), Status = "Pending" }));
        _moderationClientMock
            .Setup(c => c.UploadShopPhotosAsync(It.IsAny<IReadOnlyList<ShopPhotoBinaryPayload>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<CoffeePeek.Contract.Dtos.UploadedPhotoDto>>([]));

        return new SuggestShopViewModel(
            _catalogsClientMock.Object,
            _moderationClientMock.Object,
            _imagePickerMock.Object,
            _navigatorMock.Object);
    }

    [Fact]
    public async Task CanSubmit_False_WhenRequiredFieldsAreMissing()
    {
        var sut = CreateSut();
        await sut.InitializeAsync();

        sut.CanSubmit.Should().BeFalse();

        sut.Name = "Coffee Spot";
        sut.Address = "Somewhere 12";
        sut.CanSubmit.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitAsync_ShowsValidationError_WhenRequiredMissing()
    {
        var sut = CreateSut();
        await sut.InitializeAsync();

        await sut.SubmitCommand.ExecuteAsync(null);

        sut.ErrorMessage.Should().NotBeNullOrEmpty();
        _moderationClientMock.Verify(m => m.SendSuggestionAsync(It.IsAny<SendCoffeeShopToModerationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitAsync_CallsSendSuggestion_WhenValid()
    {
        var sut = CreateSut();
        SendCoffeeShopToModerationRequest? captured = null;
        _moderationClientMock
            .Setup(c => c.SendSuggestionAsync(It.IsAny<SendCoffeeShopToModerationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SendCoffeeShopToModerationRequest, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(Result.Ok(new SendCoffeeShopToModerationResponseDto { ShopId = Guid.NewGuid(), Status = "Pending" }));

        await sut.InitializeAsync();
        sut.Name = "Coffee Spot";
        sut.Address = "Somewhere 12";
        sut.SelectedCity = sut.Cities.First();

        await sut.SubmitCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Coffee Spot");
        captured.Address.Should().Be("Somewhere 12");
        sut.SuccessMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PickPhoto_AddsPhotoToCollection()
    {
        _imagePickerMock
            .Setup(p => p.PickImageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PickedImageFile("photo.jpg", "image/jpeg", [1, 2, 3]));

        var sut = CreateSut();

        await sut.PickPhotoCommand.ExecuteAsync(null);

        sut.Photos.Should().ContainSingle();
        sut.Photos[0].Name.Should().Be("photo.jpg");
    }
}
