using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate.Enums;

namespace CoffeePeek.Moderation.Domain.Aggregates;

public interface IQueryModerationShopRepository
{
    Task<ModerationShop?> GetById(Guid commandShopId, CancellationToken ct);
    Task<IReadOnlyList<ModerationShop>> GetAllForReviewAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<ModerationShop> Items, int TotalCount)> GetPagedForReviewAsync(
        int page,
        int pageSize,
        ModerationStatus? status,
        string? search,
        CancellationToken ct = default);
}