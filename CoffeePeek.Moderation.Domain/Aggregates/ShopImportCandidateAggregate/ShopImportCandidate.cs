using System.Globalization;
using CoffeePeek.Moderation.Domain.Import;
using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

public sealed class ShopImportCandidate : Entity<Guid>
{
    public const int GoogleCacheDays = 30;

    public ImportSource Source { get; private set; }
    public string ExternalId { get; private set; } = null!;
    public string? Name { get; private set; }
    public string? Address { get; private set; }
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string? Phone { get; private set; }
    public string? Website { get; private set; }
    public string? Instagram { get; private set; }
    public string? OpeningHours { get; private set; }
    public string? Cuisine { get; private set; }
    public string? Brand { get; private set; }
    public DateTimeOffset? OsmUpdatedAt { get; private set; }
    public int? OsmAgeDays { get; private set; }
    public string? CheckDate { get; private set; }
    public List<string> Signals { get; private set; } = [];
    public ImportCollectorBucket CollectorBucket { get; private set; }
    public ImportQueueStatus QueueStatus { get; private set; }
    public ImportCoffeeFocus? CoffeeFocus { get; private set; }
    public List<string> TagSlugs { get; private set; } = [];
    public ImportGoogleBusinessStatus? GoogleBusinessStatus { get; private set; }
    public string? GoogleMapsUri { get; private set; }
    public DateTimeOffset? GoogleFetchedAtUtc { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }
    public Guid? ResultingShopId { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private ShopImportCandidate()
    {
    }

    public bool HasRealName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            var trimmed = Name.Trim();
            return !trimmed.Equals("(unnamed)", StringComparison.OrdinalIgnoreCase)
                   && !trimmed.Equals("без имени", StringComparison.OrdinalIgnoreCase);
        }
    }

    public bool IsGoogleCacheFresh(DateTimeOffset now) =>
        GoogleFetchedAtUtc.HasValue && (now - GoogleFetchedAtUtc.Value).TotalDays <= GoogleCacheDays;

    public static ShopImportCandidate FromOsm(OsmCandidateSnapshot snapshot, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (string.IsNullOrWhiteSpace(snapshot.ExternalId))
            throw new DomainException("ExternalId is required.");

        var candidate = new ShopImportCandidate
        {
            Id = Guid.NewGuid(),
            Source = ImportSource.Osm,
            ExternalId = snapshot.ExternalId.Trim(),
            QueueStatus = ImportQueueStatus.Pending
        };
        candidate.ApplyOsmFields(snapshot, now);
        return candidate;
    }

    public void RefreshFromOsm(OsmCandidateSnapshot snapshot, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ApplyOsmFields(snapshot, now);
    }

    public void Decide(
        ImportQueueStatus status,
        ImportCoffeeFocus? focus,
        IReadOnlyList<string>? tagSlugs,
        Guid reviewerId,
        bool overrideClosed,
        DateTimeOffset now)
    {
        if (reviewerId == Guid.Empty)
            throw new DomainException("Reviewer is required.");

        if (status == ImportQueueStatus.Published)
        {
            if (focus is null)
                throw new DomainException("Coffee focus is required to publish.");

            if (!HasRealName)
                throw new DomainException("Cannot publish a candidate without a real name.");

            if (GoogleBusinessStatus == ImportGoogleBusinessStatus.ClosedPermanently && !overrideClosed)
                throw new DomainException(
                    "Google reports this place as permanently closed. Pass overrideClosed=true to publish anyway.");
        }

        QueueStatus = status;
        CoffeeFocus = status == ImportQueueStatus.Published ? focus : focus ?? CoffeeFocus;
        TagSlugs = NormalizeTagSlugs(tagSlugs, CoffeeFocus);
        ReviewedByUserId = reviewerId;
        ReviewedAtUtc = now;
    }

    public void AttachPublishedShop(Guid shopId)
    {
        if (shopId == Guid.Empty)
            throw new DomainException("ShopId cannot be empty.");

        ResultingShopId = shopId;
        QueueStatus = ImportQueueStatus.Published;
    }

    public void ApplyGoogleStatus(
        ImportGoogleBusinessStatus status,
        string? mapsUri,
        DateTimeOffset fetchedAt)
    {
        GoogleBusinessStatus = status;
        GoogleMapsUri = string.IsNullOrWhiteSpace(mapsUri) ? GoogleMapsUri : mapsUri.Trim();
        GoogleFetchedAtUtc = fetchedAt;
    }

    public ImportResearchLinks GetResearchLinks()
    {
        var display = HasRealName ? Name!.Trim() : Brand?.Trim() ?? ExternalId;
        var q = Uri.EscapeDataString($"{display} Минск");
        var qCoffee = Uri.EscapeDataString($"{display} Минск кофейня");
        var lat = Latitude.ToString(CultureInfo.InvariantCulture);
        var lon = Longitude.ToString(CultureInfo.InvariantCulture);
        var mapsQuery = Uri.EscapeDataString($"{display} {lat},{lon}");
        var (osmType, osmId) = ParseExternalId();

        return new ImportResearchLinks(
            Instagram: Instagram,
            InstagramSearch: Instagram is null
                ? $"https://www.google.com/search?q={Uri.EscapeDataString($"{display} Минск instagram")}"
                : null,
            GoogleMaps: GoogleMapsUri ?? $"https://www.google.com/maps/search/?api=1&query={mapsQuery}",
            YandexMaps: $"https://yandex.by/maps/?text={q}&z=17&ll={lon},{lat}",
            YandexImages: $"https://yandex.by/images/search?text={qCoffee}",
            OsmHistory: $"https://www.openstreetmap.org/{osmType}/{osmId}/history");
    }

    public string PublishAddress()
    {
        if (!string.IsNullOrWhiteSpace(Address))
            return Address.Trim();

        return "Минск";
    }

    private void ApplyOsmFields(OsmCandidateSnapshot snapshot, DateTimeOffset now)
    {
        var (bucket, signals, _) = OsmCafeClassifier.Classify(snapshot.Tags, snapshot.OsmUpdatedAt, now);

        Name = snapshot.Name;
        Address = snapshot.Address;
        Latitude = snapshot.Latitude;
        Longitude = snapshot.Longitude;
        Phone = snapshot.Phone;
        Website = snapshot.Website;
        Instagram = snapshot.Instagram ?? OsmCafeClassifier.InstagramUrl(snapshot.Tags, snapshot.Website);
        OpeningHours = snapshot.OpeningHours;
        Cuisine = snapshot.Cuisine;
        Brand = snapshot.Brand;
        OsmUpdatedAt = snapshot.OsmUpdatedAt;
        OsmAgeDays = snapshot.OsmUpdatedAt.HasValue
            ? (int)(now - snapshot.OsmUpdatedAt.Value).TotalDays
            : null;
        CheckDate = snapshot.CheckDate;
        Signals = signals.ToList();
        CollectorBucket = bucket;
    }

    private static List<string> NormalizeTagSlugs(IReadOnlyList<string>? tagSlugs, ImportCoffeeFocus? focus)
    {
        var slugs = (tagSlugs ?? [])
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (focus == ImportCoffeeFocus.Specialty && !slugs.Contains("specialty"))
            slugs.Add("specialty");

        if (focus != ImportCoffeeFocus.Specialty)
            slugs.Remove("specialty");

        return slugs;
    }

    private (string Type, string Id) ParseExternalId()
    {
        var parts = ExternalId.Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? (parts[0], parts[1]) : ("node", ExternalId);
    }
}
