namespace CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

public interface IShopImportDuplicateSuggestionRepository
{
    Task<ShopImportDuplicateSuggestion?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<HashSet<(Guid Left, Guid Right)>> ListPairKeysAsync(CancellationToken ct = default);

    Task<(IReadOnlyList<ShopImportDuplicateSuggestion> Items, int TotalCount)> SearchAsync(
        ImportDuplicateStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<int> CountPendingAsync(CancellationToken ct = default);

    void AddRange(IReadOnlyList<ShopImportDuplicateSuggestion> suggestions);
}
