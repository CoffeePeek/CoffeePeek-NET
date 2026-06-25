using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;
using CoffeePeek.Moderation.Domain.Common.Enums;

namespace CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;

public interface IModerationCommunityPostRepository
{
    void Add(ModerationCommunityPost post);
    Task<ModerationCommunityPost?> GetById(Guid id, CancellationToken ct = default);
}

public interface IQueryModerationCommunityPostRepository
{
    Task<ModerationCommunityPost?> GetById(Guid id, CancellationToken ct = default);

    Task<(ModerationCommunityPost[] Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        ModerationStatus? status,
        string? search,
        CancellationToken ct = default);
}
