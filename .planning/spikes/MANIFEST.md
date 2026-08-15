# Spike Manifest

## Idea

Bootstrap CoffeePeek with coffee shops in Minsk. Collect candidates from OSM (Overpass), review them in an admin queue that assigns a **Kind** (not binary specialty), then promote approved shops into the feed. Beans come later. Yandex is geocoder-only.

## Requirements

- Source v1: OSM + lists. Do not persist Yandex Places organization payloads.
- Yandex Geocoder may be used later for coordinates only.
- Admin review queue in CoffeePeek, not a spreadsheet.
- Import decision is a **category**, not yes/no specialty. Feed includes every approved Kind.
- ShopKind v1: `Specialty` | `GoodCoffee` | `Cafe` | `ToGo`. Reject stays out of the feed.
- Existing ShopTag `specialty` is a filter chip, assigned only when Kind=Specialty. Do not reuse amenity tags (laptop, pet) as Kind.
- Manual labeling on a small sample first, then autofilter from those patterns.
- Scope v1: shops only, not beans.
- Drop OSM objects not edited in **5+ years**. Show last OSM edit date on the review card.

## Spikes

| # | Name | Type | Validates | Verdict | Tags |
|---|------|------|-----------|---------|------|
| 001 | osm-overpass-minsk-cafes | standard | Given Minsk bbox, when Overpass is queried for cafe/coffee/vending, then we get a usable candidate set with measurable noise | VALIDATED | osm, overpass, import, minsk |
