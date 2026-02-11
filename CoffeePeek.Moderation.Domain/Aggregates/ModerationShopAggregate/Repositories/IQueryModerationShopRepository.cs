namespace CoffeePeek.Moderation.Domain.Aggregates;

public interface IQueryModerationShopRepository
{
    Task<ModerationShop?> GetById(Guid commandShopId, CancellationToken ct);
    Task<IReadOnlyList<ModerationShop>> GetAllForReviewAsync(CancellationToken ct = default);
}