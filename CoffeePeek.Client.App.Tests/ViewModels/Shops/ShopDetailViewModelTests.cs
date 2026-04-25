using CoffeePeek.Client.App.Core.Identity;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Shops;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using FluentAssertions;
using FluentResults;
using Moq;
using Xunit;

namespace CoffeePeek.Client.App.Tests.ViewModels.Shops;

public class ShopDetailViewModelTests
{
    private readonly Mock<IWebCoffeeShopsClient> _shopsClientMock = new();
    private readonly Mock<IWebCoffeeShopReviewsClient> _reviewsClientMock = new();
    private readonly Mock<IWorkspaceShellNavigator> _navigatorMock = new();
    private readonly Mock<IUserIdentityAccessor> _identityMock = new();
    private readonly Guid _shopId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    private ShopDetailViewModel CreateSut()
    {
        _identityMock.Setup(i => i.GetCurrentUserIdOrNull()).Returns(_userId);

        _shopsClientMock
            .Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new ShopDetailResultDto { ShopDto = BuildShopDetails() }));

        _reviewsClientMock
            .Setup(c => c.CanCreateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CanCreateCoffeeShopReviewResultDto { CanCreate = true }));

        return new ShopDetailViewModel(
            _shopsClientMock.Object,
            _reviewsClientMock.Object,
            _navigatorMock.Object,
            _identityMock.Object);
    }

    [Fact]
    public async Task SubmitReview_CallsCreateAndReloads()
    {
        _reviewsClientMock
            .Setup(c => c.CreateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CreateCoffeeShopReviewResultDto { CheckInId = Guid.NewGuid(), ReviewId = Guid.NewGuid() }));

        var sut = CreateSut();
        await sut.LoadAsync(_shopId);
        sut.NewReviewComment = "Great espresso";
        sut.PlaceScore = 4;
        sut.ServiceScore = 5;
        sut.CoffeeScore = 5;

        await sut.SubmitReviewCommand.ExecuteAsync(null);

        _reviewsClientMock.Verify(
            c => c.CreateAsync(_shopId, "Great espresso", 4, 5, 5, It.IsAny<CancellationToken>()),
            Times.Once);
        _shopsClientMock.Verify(c => c.GetByIdAsync(_shopId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteReview_DeletesOwnReviewAndReloads()
    {
        _reviewsClientMock
            .Setup(c => c.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var sut = CreateSut();
        await sut.LoadAsync(_shopId);
        var ownReview = sut.Reviews.Single(r => r.IsOwnReview);

        await sut.DeleteReviewCommand.ExecuteAsync(ownReview);

        _reviewsClientMock.Verify(c => c.DeleteAsync(ownReview.Id, It.IsAny<CancellationToken>()), Times.Once);
        _shopsClientMock.Verify(c => c.GetByIdAsync(_shopId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private CoffeeShopDetailsDto BuildShopDetails()
    {
        return new CoffeeShopDetailsDto
        {
            Id = _shopId,
            CityId = Guid.NewGuid(),
            Name = "Bean Hub",
            PriceRange = PriceRange.Moderate,
            Photos = [],
            Reviews =
            [
                new ReviewDto
                {
                    Id = Guid.NewGuid(),
                    UserId = _userId,
                    CoffeeShopId = _shopId,
                    Username = "me",
                    Header = "Nice",
                    Comment = "good",
                    Rating = new RatingDto { Place = 5, Service = 4, Coffee = 5 },
                    CreatedAtUtc = DateTime.UtcNow
                }
            ]
        };
    }
}
