using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;

public sealed class ShopMenu : Entity<Guid>
{
    public Guid CoffeeShopId { get; private set; }
    public DateTime? CapturedAtUtc { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public string Currency { get; private set; } = BusinessConstants.DefaultMenuCurrency;
    public MenuParseStatus ParseStatus { get; private set; } = MenuParseStatus.None;
    public string? ParseError { get; private set; }
    public PriceRange? SuggestedPriceRange { get; private set; }
    public string? UnmatchedJson { get; private set; }

    private readonly List<ShopMenuItem> _items = [];
    public IReadOnlyCollection<ShopMenuItem> Items => _items.AsReadOnly();

    private readonly List<ShopMenuPhoto> _photos = [];
    public IReadOnlyCollection<ShopMenuPhoto> Photos => _photos.AsReadOnly();

    // ReSharper disable once UnusedMember.Local
    private ShopMenu()
    {
    }

    public static ShopMenu Create(Guid coffeeShopId)
    {
        return new ShopMenu
        {
            Id = Guid.NewGuid(),
            CoffeeShopId = coffeeShopId,
            Currency = BusinessConstants.DefaultMenuCurrency,
            ParseStatus = MenuParseStatus.None
        };
    }

    public void MarkParsePending(DateTime capturedAtUtc)
    {
        ParseStatus = MenuParseStatus.Pending;
        ParseError = null;
        CapturedAtUtc ??= capturedAtUtc;
    }

    public void MarkParseRunning()
    {
        ParseStatus = MenuParseStatus.Running;
        ParseError = null;
    }

    public void MarkParseFailed(string error)
    {
        ParseStatus = MenuParseStatus.Failed;
        ParseError = error.Length > BusinessConstants.MaxMenuParseErrorLength
            ? error[..BusinessConstants.MaxMenuParseErrorLength]
            : error;
    }

    public void ApplyParsedItems(
        IEnumerable<ShopMenuItem> items,
        string? unmatchedJson,
        PriceRange? suggestedPriceRange,
        Guid? updatedByUserId)
    {
        _items.Clear();
        _items.AddRange(items);
        UnmatchedJson = unmatchedJson;
        SuggestedPriceRange = suggestedPriceRange;
        UpdatedByUserId = updatedByUserId;
        ParseStatus = MenuParseStatus.Ready;
        ParseError = null;
    }

    public void ReplacePhotos(IEnumerable<ShopMenuPhoto> photos)
    {
        _photos.Clear();
        _photos.AddRange(photos);
    }

    public void AddPhotos(IEnumerable<ShopMenuPhoto> photos)
    {
        foreach (var photo in photos)
        {
            if (_photos.Any(p => p.StorageKey == photo.StorageKey))
                continue;
            _photos.Add(photo);
        }
    }

    public void ApplyManualItem(
        Guid drinkDefinitionId,
        MenuItemAvailability availability,
        decimal? price,
        int? volumeMl,
        Guid? updatedByUserId)
    {
        var existing = _items.FirstOrDefault(i => i.DrinkDefinitionId == drinkDefinitionId);
        if (existing is null)
            _items.Add(ShopMenuItem.Create(drinkDefinitionId, availability, price, volumeMl, MenuItemSource.Manual));
        else
            existing.ApplyManual(availability, price, volumeMl);

        UpdatedByUserId = updatedByUserId;
        if (ParseStatus == MenuParseStatus.None)
            ParseStatus = MenuParseStatus.Ready;
    }
}
