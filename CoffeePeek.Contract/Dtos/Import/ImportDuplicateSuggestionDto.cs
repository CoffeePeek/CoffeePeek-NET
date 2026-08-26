namespace CoffeePeek.Contract.Dtos.Import;

public record ImportDuplicateCandidateDto(
    Guid Id,
    string Source,
    string ExternalId,
    string? Name,
    string? Address,
    decimal Latitude,
    decimal Longitude,
    string? Phone,
    string? Website,
    string? Instagram,
    string QueueStatus,
    bool ImportedFromFile,
    Guid? ResultingShopId);

public record ImportDuplicateSuggestionDto(
    Guid Id,
    int Score,
    double DistanceMeters,
    IReadOnlyList<string> Reasons,
    string Status,
    ImportDuplicateCandidateDto Left,
    ImportDuplicateCandidateDto Right,
    Guid? ReviewedByUserId,
    DateTimeOffset? ReviewedAtUtc);

public record GetImportDuplicatesResponse(
    IReadOnlyList<ImportDuplicateSuggestionDto> Items,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize);

public record RefreshImportDuplicatesResultDto(int Scanned, int Suggested, int AlreadyTracked);

public record DecideImportDuplicateResultDto(
    Guid SuggestionId,
    string Status,
    Guid? KeeperCandidateId,
    Guid? MergedCandidateId);
