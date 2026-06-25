using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using FluentAssertions;
using DomainComment = CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate.CommunityComment;

namespace CoffeePeek.Shops.Domain.Tests.Aggregates.CommunityCommentAggregate;

public class CommunityCommentTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var comment = DomainComment.Create(
            Guid.NewGuid(),
            "tester",
            "Nice spot!",
            CommentTargetType.Review,
            Guid.NewGuid());

        comment.Body.Should().Be("Nice spot!");
        comment.ParentCommentId.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyBody_ThrowsDomainException()
    {
        var act = () => DomainComment.Create(
            Guid.NewGuid(),
            "tester",
            "   ",
            CommentTargetType.Review,
            Guid.NewGuid());

        act.Should().Throw<Shared.Kernel.Exceptions.DomainException>();
    }

    [Fact]
    public void SoftDelete_MarksCommentDeleted()
    {
        var comment = DomainComment.Create(
            Guid.NewGuid(),
            "tester",
            "Great coffee",
            CommentTargetType.CheckIn,
            Guid.NewGuid());

        comment.SoftDelete();
        comment.IsSoftDelete.Should().BeTrue();
    }
}
