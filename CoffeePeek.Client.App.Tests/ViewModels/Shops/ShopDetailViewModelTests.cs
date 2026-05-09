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

    private ShopDetailViewModel CreateSut(bool includeOtherReview = false, bool canCreateReview = true)
    {
        _identityMock.Setup(i => i.GetCurrentUserIdOrNull()).Returns(_userId);

        _shopsClientMock
            .Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new ShopDetailResultDto { ShopDto = BuildShopDetails(includeOtherReview) }));

        _reviewsClientMock
            .Setup(c => c.CanCreateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CanCreateCoffeeShopReviewResultDto { CanCreate = canCreateReview }));

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
            .Setup(c => c.CreateAsync(It.IsAny<Guid>(), It.IsAny<CreateCoffeeShopReviewInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CreateCoffeeShopReviewResultDto { CheckInId = Guid.NewGuid(), ReviewId = Guid.NewGuid() }));

        var sut = CreateSut();
        await sut.LoadAsync(_shopId);
        sut.NewReviewComment = "Great espresso";
        sut.PlaceScore = 4;
        sut.ServiceScore = 5;
        sut.CoffeeScore = 5;

        await sut.SubmitReviewCommand.ExecuteAsync(null);

        _reviewsClientMock.Verify(
            c => c.CreateAsync(
                _shopId,
                It.Is<CreateCoffeeShopReviewInput>(i =>
                    i.Note == "Great espresso" &&
                    i.PlaceScore == 4 &&
                    i.ServiceScore == 5 &&
                    i.CoffeeScore == 5),
                It.IsAny<CancellationToken>()),
            Times.Once);
        sut.NewReviewComment.Should().BeEmpty();
        _shopsClientMock.Verify(c => c.GetByIdAsync(_shopId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SubmitReview_WhenNotLoaded_DoesNothing()
    {
        var sut = CreateSut();

        await sut.SubmitReviewCommand.ExecuteAsync(null);

        _reviewsClientMock.Verify(
            c => c.CreateAsync(It.IsAny<Guid>(), It.IsAny<CreateCoffeeShopReviewInput>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SubmitReview_WhenCannotCreate_DoesNothing()
    {
        var sut = CreateSut(canCreateReview: false);
        await sut.LoadAsync(_shopId);

        await sut.SubmitReviewCommand.ExecuteAsync(null);

        _reviewsClientMock.Verify(
            c => c.CreateAsync(It.IsAny<Guid>(), It.IsAny<CreateCoffeeShopReviewInput>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SubmitReview_WhenCreateFails_SetsReviewError()
    {
        _reviewsClientMock
            .Setup(c => c.CreateAsync(It.IsAny<Guid>(), It.IsAny<CreateCoffeeShopReviewInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<CreateCoffeeShopReviewResultDto>("Create failed"));

        var sut = CreateSut();
        await sut.LoadAsync(_shopId);

        await sut.SubmitReviewCommand.ExecuteAsync(null);

        sut.HasReviewError.Should().BeTrue();
        sut.ReviewErrorMessage.Should().Be("Create failed");
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

    [Fact]
    public async Task DeleteReview_WhenDeleteFails_SetsReviewError()
    {
        _reviewsClientMock
            .Setup(c => c.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Delete failed"));

        var sut = CreateSut();
        await sut.LoadAsync(_shopId);
        var ownReview = sut.Reviews.Single(r => r.IsOwnReview);

        await sut.DeleteReviewCommand.ExecuteAsync(ownReview);

        sut.HasReviewError.Should().BeTrue();
        sut.ReviewErrorMessage.Should().Be("Delete failed");
    }

    [Fact]
    public async Task DeleteReview_WhenReviewIsNotOwn_DoesNothing()
    {
        var sut = CreateSut(includeOtherReview: true);
        await sut.LoadAsync(_shopId);
        var otherReview = sut.Reviews.Single(r => !r.IsOwnReview);

        await sut.DeleteReviewCommand.ExecuteAsync(otherReview);

        _reviewsClientMock.Verify(c => c.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private CoffeeShopDetailsDto BuildShopDetails(bool includeOtherReview = false)
    {
        var reviews = new List<ReviewDto>
        {
            new()
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
        };

        if (includeOtherReview)
        {
            reviews.Add(new ReviewDto
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CoffeeShopId = _shopId,
                Username = "other",
                Header = "Ok",
                Comment = "fine",
                Rating = new RatingDto { Place = 4, Service = 4, Coffee = 4 },
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        return new CoffeeShopDetailsDto
        {
            Id = _shopId,
            CityId = Guid.NewGuid(),
            Name = "Bean Hub",
            PriceRange = PriceRange.Moderate,
            Photos = [],
            Reviews = reviews.ToArray()
        };
    }
}
