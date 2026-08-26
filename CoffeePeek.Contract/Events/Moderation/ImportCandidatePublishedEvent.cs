using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Events.Moderation;

public record ImportCandidatePublishedItem(
    Guid CandidateId,
    Guid CreatorId,
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    Guid CityId,
    string? Phone,
    string? Website,
    string? Instagram,
    CoffeeShopType Type,
    string[] TagSlugs,
    bool TemporarilyClosed,
    bool ImportedFromFile = false);

public record ImportCandidatePublishedEvent(IReadOnlyList<ImportCandidatePublishedItem> Items);
