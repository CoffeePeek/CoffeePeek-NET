namespace CoffeePeek.Contract.Dtos.Import;

public record ImportStatsDto(
    int Pending,
    int Skipped,
    int Published,
    int Rejected,
    int InFeed,
    int PendingDuplicates,
    IReadOnlyDictionary<string, int> ByFocus,
    IReadOnlyDictionary<string, int> ByBucket,
    ImportRejectedByReasonDto RejectedByReason);

public record ImportRejectedByReasonDto(
    int Closed,
    int Invalid,
    int NotCoffee,
    int Duplicate,
    int Unspecified);
