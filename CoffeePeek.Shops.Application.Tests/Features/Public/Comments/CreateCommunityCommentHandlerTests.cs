using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Application.Features.Public.Comments;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using FluentAssertions;
using Moq;
using DomainReview = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;
using DomainComment = CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate.CommunityComment;

namespace CoffeePeek.Shops.Application.Tests.Features.Public.Comments;

public class CreateCommunityCommentHandlerTests
{
    private readonly Mock<ICommunityCommentRepository> _commentRepoMock = new();
    private readonly Mock<IQueryReviewRepository> _reviewRepoMock = new();
    private readonly Mock<IQueryCheckInRepository> _checkInRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    [Fact]
    public async Task Handle_OnExistingReview_CreatesComment()
    {
        var shopId = Guid.NewGuid();
        var review = DomainReview.Create(shopId, Guid.NewGuid(), "author", "header", "comment body", 5, 5, 5);
        _reviewRepoMock.Setup(r => r.GetById(review.Id, _ct)).ReturnsAsync(review);

        var command = new CreateCommunityCommentCommand(
            CommunityCommentTargetType.Review,
            review.Id,
            null,
            "Totally agree!");

        var result = await CreateCommunityCommentHandler.Handle(
            command with { UserId = Guid.NewGuid(), UserName = "reader" },
            _commentRepoMock.Object,
            _reviewRepoMock.Object,
            _checkInRepoMock.Object,
            _unitOfWorkMock.Object,
            _cacheMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data.CommentId.Should().NotBe(Guid.Empty);
        _commentRepoMock.Verify(r => r.Add(It.IsAny<DomainComment>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenReplyingToReply_ThrowsDomainException()
    {
        var shopId = Guid.NewGuid();
        var review = DomainReview.Create(shopId, Guid.NewGuid(), "author", "header", "comment body", 5, 5, 5);
        var grandParentId = Guid.NewGuid();
        var parent = DomainComment.Create(
            Guid.NewGuid(), "parent", "parent body", CommentTargetType.Review, review.Id, grandParentId);

        _reviewRepoMock.Setup(r => r.GetById(review.Id, _ct)).ReturnsAsync(review);
        _commentRepoMock.Setup(r => r.GetById(parent.Id, _ct)).ReturnsAsync(parent);

        var command = new CreateCommunityCommentCommand(
            CommunityCommentTargetType.Review,
            review.Id,
            parent.Id,
            "Too deep");

        var act = async () => await CreateCommunityCommentHandler.Handle(
            command with { UserId = Guid.NewGuid(), UserName = "reader" },
            _commentRepoMock.Object,
            _reviewRepoMock.Object,
            _checkInRepoMock.Object,
            _unitOfWorkMock.Object,
            _cacheMock.Object,
            _ct);

        await act.Should().ThrowAsync<DomainException>();
    }
}
