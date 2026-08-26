using CoffeePeek.Contract.Dtos.Menu;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Events.Menu;

public enum MenuParseSourceKind
{
    ImportCandidate = 1,
    ModerationShop = 2,
    PublishedShop = 3
}

public record MenuPhotoRef(
    string FileName,
    string ContentType,
    string StorageKey,
    long SizeBytes,
    Guid? MediaPhotoId = null);

public record ParseMenuRequestedEvent(
    MenuParseSourceKind SourceKind,
    Guid SourceId,
    Guid? PublishedShopId,
    IReadOnlyList<MenuPhotoRef> Photos,
    Guid? RequestedByUserId);

public record ApplyShopMenuSnapshotEvent(
    Guid ShopId,
    ShopMenuSnapshot Snapshot,
    bool ApplySuggestedPriceRange,
    Guid? UserId);

public record MenuParsedEvent(
    MenuParseSourceKind SourceKind,
    Guid SourceId,
    Guid? PublishedShopId,
    bool Success,
    string? Error,
    PriceRange? SuggestedPriceRange,
    IReadOnlyList<ParsedMenuItemDto> Items,
    IReadOnlyList<UnmatchedMenuItemDto> Unmatched,
    DateTime CapturedAtUtc);
