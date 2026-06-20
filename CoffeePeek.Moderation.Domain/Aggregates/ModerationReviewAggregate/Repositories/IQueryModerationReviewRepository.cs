using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate.Enums;

namespace CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;

public interface IQueryModerationReviewRepository
{
    Task<ModerationReview[]> GetAll(CancellationToken cancellationToken);
    Task<ModerationReview?> GetById(Guid id, CancellationToken ct = default);
    Task<(ModerationReview[] Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        ModerationStatus? status,
        string? search,
        CancellationToken ct = default);
}