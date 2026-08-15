namespace CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

public sealed record OsmCandidateSnapshot(
    string ExternalId,
    string? Name,
    string? Address,
    decimal Latitude,
    decimal Longitude,
    string? Phone,
    string? Website,
    string? Instagram,
    string? OpeningHours,
    string? Cuisine,
    string? Brand,
    DateTimeOffset? OsmUpdatedAt,
    string? CheckDate,
    IReadOnlyDictionary<string, string> Tags);
