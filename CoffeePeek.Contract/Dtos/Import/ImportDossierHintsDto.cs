using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.Import;

public record SuggestedTagDto(string Slug, string Why);

public record ImportGapsDto(
    bool Instagram,
    bool Phone,
    bool Website,
    bool Hours,
    bool Photo);

public record YandexTagHintDto(
    string Label,
    string Slug,
    CoffeeShopType? Focus);

public record ImportDossierHintsDto(IReadOnlyList<YandexTagHintDto> YandexTags);
