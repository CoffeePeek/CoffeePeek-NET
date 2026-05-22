---
phase: 04-performance-optimization
plan: "01"
subsystem: CoffeePeek.Shops.Persistance
tags: [performance, ef-core, query-optimization]
dependency_graph:
  requires: []
  provides: [join-based-min-rating-filter, include-free-favorites-query]
  affects: [CoffeePeek.Shops.Persistance]
tech_stack:
  added: []
  patterns: [LINQ GroupBy+Average+Join, Mapster ProjectToType without Include]
key_files:
  modified:
    - CoffeePeek.Shops.Persistance/Queries/CoffeeShopQueries.cs
decisions:
  - "MinRating uses INNER JOIN to pre-aggregated subquery — shops with no reviews excluded (consistent with previous FirstOrDefault() returning 0.0)"
  - "GetUserFavoriteCoffeeShops drops Include — Mapster ProjectToType owns the JOIN"
metrics:
  duration: "~10 minutes"
  completed: "2026-05-20"
  tasks_total: 2
  tasks_completed: 2
  files_modified: 1
---

# Phase 04 Plan 01: Query Bottleneck Fixes Summary

Eliminated two EF Core query performance bottlenecks in CoffeeShopQueries.cs.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Rewrite MinRating filter as LINQ Join (PERF-01) | dd53d51 | CoffeePeek.Shops.Persistance/Queries/CoffeeShopQueries.cs |
| 2 | Remove redundant Include from GetUserFavoriteCoffeeShops (PERF-03) | a7c243e | CoffeePeek.Shops.Persistance/Queries/CoffeeShopQueries.cs |

## Changes Made

### Task 1 — MinRating Filter (PERF-01)

Replaced correlated subquery (FirstOrDefault per outer row) with LINQ Join to a pre-aggregated GroupBy subquery. EF Core now emits a single INNER JOIN against a derived table instead of a subselect per shop.

### Task 2 — GetUserFavoriteCoffeeShops (PERF-03)

Removed `.Include(x => x.CoffeeShop)` from the query chain. Mapster's `ProjectToType<CoffeeShopDetailsDto>()` generates its own JOIN; the Include was redundant and forced EF Core to eagerly load tracked entities.

## Verification

- `dotnet build CoffeePeek.slnx` — 0 errors (87 pre-existing XML doc warnings, unrelated to these changes)
- `dotnet test` across all 4 test projects — 365 tests passed, 0 failed
- `grep "FirstOrDefault"` in CoffeeShopQueries.cs — 0 matches in MinRating block (only `FirstOrDefaultAsync` terminal operator in GetDetailsById remains, which is correct)
- `grep ".Include("` in CoffeeShopQueries.cs — 0 matches

## Deviations from Plan

None.

## Known Stubs

None.

## Threat Flags

None — no new network endpoints, auth paths, or schema changes introduced.

## Self-Check: PASSED
