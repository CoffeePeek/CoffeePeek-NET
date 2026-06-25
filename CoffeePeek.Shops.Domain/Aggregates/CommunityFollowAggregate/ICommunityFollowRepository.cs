namespace CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;

public interface ICommunityUserFollowRepository
{
    void Add(CommunityUserFollow follow);
    void Remove(CommunityUserFollow follow);
    Task<CommunityUserFollow?> GetAsync(Guid followerId, Guid followingUserId, CancellationToken ct = default);
}

public interface IQueryCommunityUserFollowRepository
{
    Task<IReadOnlyList<Guid>> GetFollowingUserIdsAsync(Guid followerId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetFollowerUserIdsAsync(Guid followingUserId, CancellationToken ct = default);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followingUserId, CancellationToken ct = default);
}

public interface ICommunityCityFollowRepository
{
    void Add(CommunityCityFollow follow);
    void Remove(CommunityCityFollow follow);
    Task<CommunityCityFollow?> GetAsync(Guid userId, Guid cityId, CancellationToken ct = default);
}

public interface IQueryCommunityCityFollowRepository
{
    Task<IReadOnlyList<Guid>> GetFollowedCityIdsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> IsFollowingCityAsync(Guid userId, Guid cityId, CancellationToken ct = default);
}
