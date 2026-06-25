using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Shops.Application.Features.Review.CanCreateCoffeeShopReview;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using FluentAssertions;
using Moq;

namespace CoffeePeek.Shops.Application.Tests.Features.Review.CanCreateCoffeeShopReview;

public class CanCreateCoffeeShopReviewHandlerTests
{
    private readonly Mock<IQueryReviewRepository> _reviewRepositoryMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    [Fact]
    public async Task Handle_WhenNoActiveReview_ReturnsCanCreateTrue()
    {
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _reviewRepositoryMock
            .Setup(r => r.ExistsForCurrentUser(shopId, userId, _ct))
            .ReturnsAsync((Guid?)null);

        var handler = new CanCreateCoffeeShopReviewHandler();
        var response = await handler.Handle(
            new CanCreateCoffeeShopReviewQuery(userId, shopId),
            _reviewRepositoryMock.Object,
            _ct);

        response.IsSuccess.Should().BeTrue();
        response.Data.CanCreate.Should().BeTrue();
        response.Data.ReviewId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenActiveReviewExists_ReturnsCanCreateFalseWithReviewId()
    {
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();

        _reviewRepositoryMock
            .Setup(r => r.ExistsForCurrentUser(shopId, userId, _ct))
            .ReturnsAsync(reviewId);

        var handler = new CanCreateCoffeeShopReviewHandler();
        var response = await handler.Handle(
            new CanCreateCoffeeShopReviewQuery(userId, shopId),
            _reviewRepositoryMock.Object,
            _ct);

        response.IsSuccess.Should().BeTrue();
        response.Data.CanCreate.Should().BeFalse();
        response.Data.ReviewId.Should().Be(reviewId);
    }
}
