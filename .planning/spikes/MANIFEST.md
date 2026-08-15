# Spike Manifest

## Idea

Bootstrap CoffeePeek with specialty coffee shops in Minsk. Collect candidates from OSM (Overpass), review them in an admin yes/no queue, then promote only approved shops into the catalog. Beans come later. Yandex is geocoder-only.

## Requirements

- Source v1: OSM + lists. Do not persist Yandex Places organization payloads.
- Yandex Geocoder may be used later for coordinates only.
- Admin review queue in CoffeePeek (yes/no), not a spreadsheet.
- Manual labeling on a small sample first, then autofilter from those patterns.
- Scope v1: shops only, not beans.

## Spikes

| # | Name | Type | Validates | Verdict | Tags |
|---|------|------|-----------|---------|------|
| 001 | osm-overpass-minsk-cafes | standard | Given Minsk bbox, when Overpass is queried for cafe/coffee/vending, then we get a usable candidate set with measurable noise | VALIDATED | osm, overpass, import, minsk |
