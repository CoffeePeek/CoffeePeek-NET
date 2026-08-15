namespace CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

public interface IShopImportCandidateRepository
{
    Task<ShopImportCandidate?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyDictionary<string, ShopImportCandidate>> GetByExternalIdsAsync(
        ImportSource source,
        IReadOnlyCollection<string> externalIds,
        CancellationToken ct = default);

    void Add(ShopImportCandidate candidate);

    Task<(IReadOnlyList<ShopImportCandidate> Items, int TotalCount)> SearchAsync(
        ImportQueueStatus? status,
        ImportCollectorBucket? bucket,
        ImportCoffeeFocus? focus,
        ImportRejectReason? rejectReason,
        string? search,
        bool excludeStale,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<ImportCandidateStats> GetStatsAsync(CancellationToken ct = default);
}

public sealed record ImportCandidateStats(
    int Pending,
    int Skipped,
    int Published,
    int Rejected,
    int InFeed,
    IReadOnlyDictionary<ImportCoffeeFocus, int> ByFocus,
    IReadOnlyDictionary<ImportCollectorBucket, int> ByBucket,
    ImportRejectedByReasonStats RejectedByReason);

public sealed record ImportRejectedByReasonStats(
    int Closed,
    int Invalid,
    int NotCoffee,
    int Unspecified);
