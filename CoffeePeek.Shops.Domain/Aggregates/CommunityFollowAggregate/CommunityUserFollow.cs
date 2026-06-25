using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;

public sealed class CommunityUserFollow : Entity<Guid>
{
    public Guid FollowerId { get; private set; }
    public Guid FollowingUserId { get; private set; }

    private CommunityUserFollow() { }

    private CommunityUserFollow(Guid followerId, Guid followingUserId)
    {
        Id = Guid.NewGuid();
        FollowerId = followerId;
        FollowingUserId = followingUserId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static CommunityUserFollow Create(Guid followerId, Guid followingUserId)
    {
        if (followerId == Guid.Empty)
            throw new DomainException("FollowerId cannot be empty.");

        if (followingUserId == Guid.Empty)
            throw new DomainException("FollowingUserId cannot be empty.");

        if (followerId == followingUserId)
            throw new DomainException("You cannot follow yourself.");

        return new CommunityUserFollow(followerId, followingUserId);
    }
}
