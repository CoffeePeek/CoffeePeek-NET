namespace CoffeePeek.Contract.Dtos.Import;

public record ImportResearchLinksDto(
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
