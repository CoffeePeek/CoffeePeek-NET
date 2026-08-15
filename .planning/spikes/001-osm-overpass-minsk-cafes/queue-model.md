# Draft: import queue model (not implemented)

`ModerationShop` is the wrong home for OSM ingest.

It is an **owner-submitted** shop: required `UserId`, optional catalog links (beans, roasters, brew methods), photos, schedules. Import candidates have an external source, no owner, and incomplete fields.

## Proposed aggregate

`ShopImportCandidate` in Moderation (or a thin Import module):

```csharp
public sealed class ShopImportCandidate : Entity<Guid>
{
    public ImportSource Source { get; }          // Osm
    public string ExternalId { get; }            // "node/123"
    public string Name { get; }
    public string? Address { get; }
    public double Latitude { get; }
    public double Longitude { get; }
    public string? Phone { get; }
    public string? Website { get; }
    public string? OpeningHours { get; }
    public ImportBucket Bucket { get; }          // LikelySpecialty, Priority, Review, LikelyNoise, AutoReject
    public IReadOnlyList<string> Signals { get; }
    public string RawTagsJson { get; }
    public ImportDecision Decision { get; }      // Pending, Approved, Rejected, Skipped
    public ShopKind? Kind { get; }               // set on approve
    public Guid? ReviewedByUserId { get; }
    public DateTimeOffset? ReviewedAtUtc { get; }
    public Guid? ResultingShopId { get; }
}

public enum ShopKind
{
    Specialty = 1,   // third wave / roastery — also assign existing ShopTag `specialty`
    GoodCoffee = 2,  // decent coffee, not claiming specialty — still in the feed
    Cafe = 3,        // food-first place with coffee worth listing
    ToGo = 4,        // takeaway / chain with drinkable espresso
}
// Reject is Decision=Rejected, not a Kind. Only Kind values go to the feed.
```

Unique index: `(Source, ExternalId)`.

## Flow

1. Collector upserts OSM snapshot → `Pending`.
2. Admin assigns a **Kind** (or reject) on the queue. Start with `LikelySpecialty` + `Priority`.
3. **Approve** creates a `CoffeeShop` with that `Kind` (system actor), stores `ResultingShopId`. Do not invent a fake shop owner. `specialty` ShopTag is assigned only when Kind=Specialty. Other amenity tags (laptop, pet) stay orthogonal.
4. **Reject** stays out of the catalog; keep the row so re-fetch does not resurrect it.
5. After ~50–100 manual labels, train/tune autofilter from `Signals` + `Decision`.

## What OSM already gives vs gaps

| Field | OSM coverage in this spike | Action |
|-------|----------------------------|--------|
| name | 1455 / 1576 | skip unnamed or fill from brand |
| lat/lon | 1576 / 1576 | enough; Yandex Geocoder only if we later want a formatted address |
| address | 528 / 1576 | optional on candidate; geocode later |
| website / phone / hours | partial | copy if present |
| specialty? | almost never tagged | **human decision** |

## Do not

- Reuse `ModerationShop.Create(userId, ...)` for imports.
- Persist Yandex Places organization payloads.
- Auto-approve `amenity=cafe` — that bucket is mostly canteens, pizza, and generic cafes.
