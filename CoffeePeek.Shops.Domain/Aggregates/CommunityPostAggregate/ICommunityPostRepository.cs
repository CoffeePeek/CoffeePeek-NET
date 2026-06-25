using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;

namespace CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;

public interface ICommunityPostRepository
{
    void Add(CommunityPost post);
    Task<bool> ExistsByModerationPostIdAsync(Guid moderationPostId, CancellationToken ct = default);
}

public interface IQueryCommunityPostRepository
{
    Task<bool> ExistsByIdAsync(Guid postId, CancellationToken ct = default);
    Task<CommunityPost?> GetByIdAsync(Guid postId, CancellationToken ct = default);
}
