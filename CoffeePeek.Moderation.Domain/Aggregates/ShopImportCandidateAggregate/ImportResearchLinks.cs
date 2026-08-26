namespace CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

public readonly record struct ImportResearchLinks(
    string? Instagram,
    string? InstagramSearch,
    string GoogleMaps,
    string YandexMaps,
    string YandexImages,
    string OsmHistory,
    string YandexEmbed,
    string GoogleEmbed,
    string StreetView,
    string StreetViewEmbed);
