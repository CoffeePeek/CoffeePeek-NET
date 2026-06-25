using CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;
using FluentAssertions;

namespace CoffeePeek.Shops.Domain.Tests.Aggregates.CommunityFollow;

public class CommunityUserFollowTests
{
    [Fact]
    public void Create_WithValidUsers_Succeeds()
    {
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();

        var follow = CommunityUserFollow.Create(followerId, followingId);

        follow.FollowerId.Should().Be(followerId);
        follow.FollowingUserId.Should().Be(followingId);
    }

    [Fact]
    public void Create_WhenFollowingSelf_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();

        var act = () => CommunityUserFollow.Create(userId, userId);

        act.Should().Throw<Shared.Kernel.Exceptions.DomainException>();
    }
}
