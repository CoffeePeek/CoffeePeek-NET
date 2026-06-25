using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Community;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Application.Features.Public.Reactions;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using FluentAssertions;
using Moq;
using DomainReview = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;
using DomainReactionType = CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate.CommunityReactionType;

namespace CoffeePeek.Shops.Application.Tests.Features.Public.Reactions;

public class SetCommunityReactionHandlerTests
{
    private readonly Mock<ICommunityReactionRepository> _reactionRepoMock = new();
    private readonly Mock<IQueryReviewRepository> _reviewRepoMock = new();
    private readonly Mock<IQueryCheckInRepository> _checkInRepoMock = new();
    private readonly Mock<IQueryCommunityPostRepository> _postRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    [Fact]
    public async Task Handle_OnNewReaction_CreatesReactionAndEmitsNotification()
    {
        var shopId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var review = DomainReview.Create(shopId, authorId, "author", "header", "comment body", 5, 5, 5);

        _reviewRepoMock.Setup(r => r.GetById(review.Id, _ct)).ReturnsAsync(review);
        _reactionRepoMock
            .Setup(r => r.GetByUserAndTargetAsync(It.IsAny<Guid>(), ReactionTargetType.Review, review.Id, _ct))
            .ReturnsAsync((CommunityReaction?)null);

        var reactorId = Guid.NewGuid();
        var command = new SetCommunityReactionCommand(
            CommunityCommentTargetType.Review,
            review.Id,
            Contract.Enums.CommunityReactionType.Helpful)
        {
            UserId = reactorId,
            UserName = "reactor"
        };

        var (result, notification) = await SetCommunityReactionHandler.Handle(
            command,
            _reactionRepoMock.Object,
            _reviewRepoMock.Object,
            _checkInRepoMock.Object,
            _postRepoMock.Object,
            _unitOfWorkMock.Object,
            _cacheMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data.ActiveReaction.Should().Be(Contract.Enums.CommunityReactionType.Helpful);
        notification.Should().NotBeNull();
        notification!.RecipientUserId.Should().Be(authorId);
        _reactionRepoMock.Verify(r => r.Add(It.IsAny<CommunityReaction>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTogglingSameReaction_RemovesWithoutNotification()
    {
        var shopId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var review = DomainReview.Create(shopId, authorId, "author", "header", "comment body", 5, 5, 5);
        var existing = CommunityReaction.Create(
            authorId,
            ReactionTargetType.Review,
            review.Id,
            DomainReactionType.Helpful);

        _reviewRepoMock.Setup(r => r.GetById(review.Id, _ct)).ReturnsAsync(review);
        _reactionRepoMock
            .Setup(r => r.GetByUserAndTargetAsync(authorId, ReactionTargetType.Review, review.Id, _ct))
            .ReturnsAsync(existing);

        var command = new SetCommunityReactionCommand(
            CommunityCommentTargetType.Review,
            review.Id,
            Contract.Enums.CommunityReactionType.Helpful)
        {
            UserId = authorId,
            UserName = "author"
        };

        var (result, notification) = await SetCommunityReactionHandler.Handle(
            command,
            _reactionRepoMock.Object,
            _reviewRepoMock.Object,
            _checkInRepoMock.Object,
            _postRepoMock.Object,
            _unitOfWorkMock.Object,
            _cacheMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data.WasRemoved.Should().BeTrue();
        notification.Should().BeNull();
        _reactionRepoMock.Verify(r => r.Remove(existing), Times.Once);
    }
}
