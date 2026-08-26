namespace CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

public sealed record CoffeeMapCandidateSnapshot(
    string ExternalId,
    string? Name,
    string? Address,
    decimal Latitude,
    decimal Longitude,
    string? Phone,
    string? Website,
    string? Instagram,
    string? OpeningHours,
    string? GooglePlaceId,
    bool IsSpecialty,
    bool Recommended,
    double? GoogleRating,
    int? GoogleRatingsCount,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<string> AmenitySignals);
