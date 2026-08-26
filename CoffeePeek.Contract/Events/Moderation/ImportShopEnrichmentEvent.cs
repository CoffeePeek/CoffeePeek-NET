namespace CoffeePeek.Contract.Events.Moderation;

public record ImportShopEnrichmentItem(
    Guid? ShopId,
    string Name,
    string? Address,
    decimal Latitude,
    decimal Longitude,
    string? Phone,
    string? Website,
    string? Instagram);

public record ImportShopEnrichmentEvent(IReadOnlyList<ImportShopEnrichmentItem> Items);
