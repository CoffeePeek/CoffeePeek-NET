using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.Menu;

public record ShopMenuItemSnapshot(
    string Slug,
    MenuItemAvailability Availability,
    decimal? Price,
    int? VolumeMl,
    MenuItemSource Source);

public record ShopMenuPhotoSnapshot(
    string FileName,
    string ContentType,
    string StorageKey,
    long SizeBytes,
    Guid? MediaPhotoId = null);

public record ShopMenuSnapshot(
    DateTime? CapturedAtUtc,
    string Currency,
    PriceRange? SuggestedPriceRange,
    IReadOnlyList<ShopMenuItemSnapshot> Items,
    IReadOnlyList<ShopMenuPhotoSnapshot> Photos,
    string? UnmatchedJson = null);
