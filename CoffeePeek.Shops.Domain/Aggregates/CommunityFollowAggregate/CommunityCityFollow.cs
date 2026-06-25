using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;

public sealed class CommunityCityFollow : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid CityId { get; private set; }

    private CommunityCityFollow() { }

    private CommunityCityFollow(Guid userId, Guid cityId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CityId = cityId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static CommunityCityFollow Create(Guid userId, Guid cityId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (cityId == Guid.Empty)
            throw new DomainException("CityId cannot be empty.");

        return new CommunityCityFollow(userId, cityId);
    }
}
