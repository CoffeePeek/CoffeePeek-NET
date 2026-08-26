namespace CoffeePeek.Moderation.Domain.Aggregates.MenuDraftAggregate;

public sealed class MenuDraft
{
    public const int MaxPhotos = 4;
    public const string DefaultCurrency = "BYN";
    public const int MaxParseErrorLength = 1000;

    public DateTime? CapturedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string Currency { get; set; } = DefaultCurrency;
    public int ParseStatus { get; set; }
    public string? ParseError { get; set; }
    public int? SuggestedPriceRange { get; set; }
    public List<MenuDraftItem> Items { get; set; } = [];
    public List<MenuDraftPhoto> Photos { get; set; } = [];
    public List<MenuDraftUnmatched> Unmatched { get; set; } = [];

    public static MenuDraft CreateEmpty() =>
        new()
        {
            Currency = DefaultCurrency,
            ParseStatus = (int)MenuDraftParseStatus.None
        };

    public void AttachPhotos(
        IReadOnlyList<(string FileName, string ContentType, string StorageKey, long SizeBytes)> photos,
        DateTime utcNow)
    {
        foreach (var photo in photos)
        {
            if (string.IsNullOrWhiteSpace(photo.StorageKey))
                continue;
            if (Photos.Any(p => p.StorageKey == photo.StorageKey))
                continue;
            if (Photos.Count >= MaxPhotos)
                break;

            Photos.Add(new MenuDraftPhoto
            {
                Id = Guid.NewGuid(),
                FileName = photo.FileName,
                ContentType = photo.ContentType,
                StorageKey = photo.StorageKey,
                SizeBytes = photo.SizeBytes
            });
        }

        CapturedAtUtc ??= utcNow;
        UpdatedAtUtc = utcNow;
        ParseStatus = (int)MenuDraftParseStatus.Pending;
        ParseError = null;
    }

    public void MarkParsePending()
    {
        ParseStatus = (int)MenuDraftParseStatus.Pending;
        ParseError = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ApplyParseResult(
        bool success,
        string? error,
        int? suggestedPriceRange,
        IReadOnlyList<MenuDraftItem> items,
        IReadOnlyList<MenuDraftUnmatched> unmatched,
        DateTime utcNow)
    {
        UpdatedAtUtc = utcNow;
        if (!success)
        {
            ParseStatus = (int)MenuDraftParseStatus.Failed;
            ParseError = ClipError(error);
            return;
        }

        Items = items.ToList();
        Unmatched = unmatched.ToList();
        SuggestedPriceRange = suggestedPriceRange;
        ParseStatus = (int)MenuDraftParseStatus.Ready;
        ParseError = null;
    }

    public void ApplyManualItems(IReadOnlyList<MenuDraftItem> edits, DateTime utcNow)
    {
        foreach (var edit in edits)
        {
            if (string.IsNullOrWhiteSpace(edit.Slug))
                continue;

            var existing = Items.FirstOrDefault(i =>
                string.Equals(i.Slug, edit.Slug, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                Items.Add(edit);
                continue;
            }

            existing.Availability = edit.Availability;
            existing.Price = edit.Availability == (int)MenuDraftAvailability.Present ? edit.Price : null;
            existing.VolumeMl = edit.VolumeMl;
            existing.Source = (int)MenuDraftItemSource.Manual;
            if (!string.IsNullOrWhiteSpace(edit.NameRu))
                existing.NameRu = edit.NameRu;
            if (!string.IsNullOrWhiteSpace(edit.NameEn))
                existing.NameEn = edit.NameEn;
            if (edit.Category != 0)
                existing.Category = edit.Category;
        }

        UpdatedAtUtc = utcNow;
        if (ParseStatus == (int)MenuDraftParseStatus.None)
            ParseStatus = (int)MenuDraftParseStatus.Ready;
    }

    private static string? ClipError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return error;
        return error.Length <= MaxParseErrorLength ? error : error[..MaxParseErrorLength];
    }
}

public sealed class MenuDraftItem
{
    public string Slug { get; set; } = "";
    public string NameRu { get; set; } = "";
    public string NameEn { get; set; } = "";
    public int Category { get; set; }
    public int Availability { get; set; }
    public decimal? Price { get; set; }
    public int? VolumeMl { get; set; }
    public int Source { get; set; }
}

public sealed class MenuDraftPhoto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public string StorageKey { get; set; } = "";
    public long SizeBytes { get; set; }
    public Guid? MediaPhotoId { get; set; }
}

public sealed class MenuDraftUnmatched
{
    public string RawName { get; set; } = "";
    public decimal? Price { get; set; }
    public double? Confidence { get; set; }
}

public enum MenuDraftParseStatus
{
    None = 0,
    Pending = 1,
    Running = 2,
    Ready = 3,
    Failed = 4
}

public enum MenuDraftAvailability
{
    Unknown = 0,
    Present = 1,
    Absent = 2
}

public enum MenuDraftItemSource
{
    Parsed = 1,
    Manual = 2
}
