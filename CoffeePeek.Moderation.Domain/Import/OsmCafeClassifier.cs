using System.Text.RegularExpressions;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

namespace CoffeePeek.Moderation.Domain.Import;

public static partial class OsmCafeClassifier
{
    public const int StaleYears = 5;

    [GeneratedRegex(@"specialty|спеш[еа]лти|third.?wave|thirdwave|обжар|roaster|brew bar|brewbar", RegexOptions.IgnoreCase)]
    private static partial Regex SpecialtyName();

    [GeneratedRegex(@"starbucks|mcdonald|kfc|burger king|costa coffee|dunkin|шоколадница|кофеин[аы]?$|coffeeshop company|gloria jean|costa\b", RegexOptions.IgnoreCase)]
    private static partial Regex ChainName();

    [GeneratedRegex(@"автомат|vending|coffee.?point|кофе.?поинт|кофепоинт|coffee machine|кофемат|кофейный аппарат", RegexOptions.IgnoreCase)]
    private static partial Regex VendingName();

    [GeneratedRegex(@"столов|буфет|лидо|mcdonald|макдонал|kfc|burger king", RegexOptions.IgnoreCase)]
    private static partial Regex CanteenName();

    [GeneratedRegex(@"^varka\b|варка coffee|cofix|шоколадница|coffeeshop company|cinnabon|mccaf", RegexOptions.IgnoreCase)]
    private static partial Regex ToGoChain();

    public static (ImportCollectorBucket Bucket, IReadOnlyList<string> Signals, bool Stale) Classify(
        IReadOnlyDictionary<string, string> tags,
        DateTimeOffset? osmUpdatedAt,
        DateTimeOffset now)
    {
        var signals = new List<string>();
        var stale = osmUpdatedAt.HasValue && (now - osmUpdatedAt.Value).TotalDays > StaleYears * 365;
        if (stale)
            signals.Add($"osm:stale>{StaleYears}y");

        var amenity = Get(tags, "amenity");
        var shop = Get(tags, "shop");
        var cuisine = (Get(tags, "cuisine") ?? "").ToLowerInvariant();
        var name = Get(tags, "name") ?? Get(tags, "name:ru") ?? Get(tags, "name:en") ?? "";
        var brand = (Get(tags, "brand") ?? "").ToLowerInvariant();

        ImportCollectorBucket bucket;
        if (amenity == "vending_machine" || Get(tags, "vending") == "coffee")
        {
            signals.Add("osm:vending_machine");
            bucket = ImportCollectorBucket.AutoReject;
        }
        else if (VendingName().IsMatch(name))
        {
            signals.Add("name:vending-like");
            bucket = ImportCollectorBucket.AutoReject;
        }
        else if (CanteenName().IsMatch(name))
        {
            signals.Add("name:canteen");
            bucket = ImportCollectorBucket.LikelyNoise;
        }
        else if (ToGoChain().IsMatch(name) || brand is "varka" or "cofix" or "шоколадница" or "starbucks" or "mcdonald's" or "kfc")
        {
            signals.Add("name:to-go-chain");
            bucket = ImportCollectorBucket.LikelyNoise;
        }
        else if (ChainName().IsMatch(name))
        {
            signals.Add("name:chain");
            bucket = ImportCollectorBucket.LikelyNoise;
        }
        else if (SpecialtyName().IsMatch(name))
        {
            signals.Add("name:specialty-signal");
            bucket = ImportCollectorBucket.LikelySpecialty;
        }
        else if (shop == "coffee")
        {
            signals.Add("osm:shop=coffee");
            bucket = ImportCollectorBucket.Priority;
        }
        else if (cuisine.Contains("coffee_shop") || cuisine == "coffee")
        {
            signals.Add($"osm:cuisine={cuisine}");
            bucket = ImportCollectorBucket.Priority;
        }
        else if (name.Contains("кофе", StringComparison.OrdinalIgnoreCase) ||
                 name.Contains("coffee", StringComparison.OrdinalIgnoreCase))
        {
            signals.Add("name:coffee");
            bucket = ImportCollectorBucket.Priority;
        }
        else
        {
            signals.Add("osm:amenity=cafe");
            bucket = ImportCollectorBucket.Review;
        }

        if (stale)
            bucket = ImportCollectorBucket.Stale;

        return (bucket, signals, stale);
    }

    public static string? InstagramUrl(IReadOnlyDictionary<string, string> tags, string? website)
    {
        var raw = Get(tags, "contact:instagram") ?? Get(tags, "instagram");
        if (raw is null && website is not null && website.Contains("instagram.com", StringComparison.OrdinalIgnoreCase))
            raw = website;
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        raw = raw.Trim().Replace("instgram.com", "instagram.com", StringComparison.OrdinalIgnoreCase);
        if (raw.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return raw;
        return $"https://www.instagram.com/{raw.TrimStart('@')}/";
    }

    private static string? Get(IReadOnlyDictionary<string, string> tags, string key) =>
        tags.TryGetValue(key, out var value) ? value : null;
}
