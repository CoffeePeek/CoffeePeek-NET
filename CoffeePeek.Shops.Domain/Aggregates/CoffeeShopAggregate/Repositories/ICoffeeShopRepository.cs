namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public record UserShopEnrichment(bool IsFavorite, bool IsVisited, Guid? ExistingReviewId);

public interface ICoffeeShopRepository
{
    Task<bool> Exists(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetShopNamesByIdsAsync(IEnumerable<Guid> shopIds, CancellationToken ct = default);
    Task<UserShopEnrichment> GetUserShopEnrichmentAsync(Guid userId, Guid shopId, CancellationToken ct = default);

    Task<Dictionary<Guid, UserShopEnrichment>> GetBatchUserShopEnrichmentAsync(Guid userId, IEnumerable<Guid> shopIds,
        CancellationToken ct = default);
}