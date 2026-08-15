namespace CoffeePeek.Contract.Dtos.Import;

public record ImportStatsDto(
    int Pending,
    int Skipped,
    int Published,
    int Rejected,
    int InFeed,
    IReadOnlyDictionary<string, int> ByFocus,
    IReadOnlyDictionary<string, int> ByBucket);
