using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Home;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using FluentAssertions;
using FluentResults;
using Moq;
using Xunit;

namespace CoffeePeek.Client.App.Tests.ViewModels.Home;

public class ModerationPanelViewModelTests
{
    private readonly Mock<IWebModerationPanelClient> _clientMock = new();
    private readonly Mock<IWorkspaceShellNavigator> _navigatorMock = new();

    private ModerationPanelViewModel CreateSut() =>
        new(_clientMock.Object, _navigatorMock.Object);

    [Fact]
    public async Task LoadAsync_PopulatesLists()
    {
        var shop = new ModerationShopDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Cafe",
            Schedules = [],
            EquipmentIds = [],
            CoffeeBeanIds = [],
            RoasterIds = [],
            BrewMethodIds = [],
            ShopPhotos = [],
            ModerationStatus = ModerationStatus.Pending
        };
        _clientMock
            .Setup(c => c.GetAllShopsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new[] { shop }));
        _clientMock
            .Setup(c => c.GetAllReviewsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Array.Empty<ModerationReviewDto>()));

        var sut = CreateSut();
        await sut.LoadAsync();

        sut.Shops.Should().ContainSingle();
        sut.Reviews.Should().BeEmpty();
        sut.HasEmptyReviews.Should().BeTrue();
        sut.HasEmptyShops.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_WhenShopsFail_SetsError_And_ClearsLoading()
    {
        _clientMock
            .Setup(c => c.GetAllShopsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<ModerationShopDto[]>("x"));
        _clientMock
            .Setup(c => c.GetAllReviewsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Array.Empty<ModerationReviewDto>()));

        var sut = CreateSut();
        await sut.LoadAsync();

        sut.ErrorMessage.Should().NotBeNullOrEmpty();
        sut.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_WhenReviewsFail_SetsError()
    {
        _clientMock
            .Setup(c => c.GetAllShopsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Array.Empty<ModerationShopDto>()));
        _clientMock
            .Setup(c => c.GetAllReviewsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<ModerationReviewDto[]>("x"));

        var sut = CreateSut();
        await sut.LoadAsync();

        sut.ErrorMessage.Should().NotBeNullOrEmpty();
        sut.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_InvokesShopsAndReviews()
    {
        _clientMock
            .Setup(c => c.GetAllShopsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Array.Empty<ModerationShopDto>()));
        _clientMock
            .Setup(c => c.GetAllReviewsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Array.Empty<ModerationReviewDto>()));

        var sut = CreateSut();
        await sut.LoadAsync();

        _clientMock.Verify(c => c.GetAllShopsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _clientMock.Verify(c => c.GetAllReviewsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Close_CallsNavigator()
    {
        var sut = CreateSut();
        sut.CloseCommand.Execute(null);
        _navigatorMock.Verify(n => n.CloseModerationPanel(), Times.Once);
    }
}
