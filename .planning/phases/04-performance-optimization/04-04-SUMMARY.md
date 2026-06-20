---
plan: 04-04
phase: 04-performance-optimization
status: complete
completed: "2026-05-20"
duration_estimate: 10m
tasks_completed: 2
files_modified: 5
requirements_closed:
  - PERF-02
---

# Plan 04-04 — Summary: ILike Search + pg_trgm GIN Indexes (PERF-02)

## What Was Done

Replaced the full sequential-scan search predicate with index-accelerated `ILIKE` and added PostgreSQL GIN trigram indexes.

### Task 1 — Code changes (3 files)

**CoffeeShopQueries.cs** — Search predicate rewritten:
```
// Before (generates LOWER(column) LIKE '%term%' — un-indexable)
var term = request.Query.Trim().ToLower();
query = query.Where(s => s.Name.ToLower().Contains(term) || s.Location.Address.ToLower().Contains(term));

// After (EF.Functions.ILike uses PostgreSQL ILIKE — GIN-accelerated)
var term = $"%{request.Query.Trim()}%";
query = query.Where(s => EF.Functions.ILike(s.Name, term) || EF.Functions.ILike(s.Location.Address, term));
```

**ShopsDbContext.cs** — `HasPostgresExtension("pg_trgm")` added in `OnModelCreating` before `ApplyConfiguration`.

**CoffeeShopConfiguration.cs** — Two GIN indexes declared:
- `IX_Shops_Name_GIN` on `Name` (root `builder`)
- `IX_Shops_Address_GIN` on `Location.Address` (inside `OwnsOne` closure on `location` builder)

### Task 2 — Migration (checkpoint: human-verified)

EF Core migration generated and applied as part of the `Update2025` batch migration (commit `8b33c45`). Migration `20260520052808_Update2025.cs` contains:
- `Npgsql:PostgresExtension:pg_trgm` annotation
- `IX_Shops_Name_GIN` index with `gin_trgm_ops`
- `IX_Shops_Address_GIN` index with `gin_trgm_ops`

## Commits

- `31515f2` — perf(04-04): add pg_trgm GIN indexes and ILike search predicate (PERF-02)
- `8b33c45` — +add Update2025 migration (includes Shops pg_trgm migration)

## Verification

- [x] `EF.Functions.ILike` in `CoffeeShopQueries.cs`; `.ToLower().Contains(` absent
- [x] `HasPostgresExtension("pg_trgm")` in `ShopsDbContext.OnModelCreating`
- [x] `gin_trgm_ops` in `CoffeeShopConfiguration.cs` (2 occurrences: Name + Address)
- [x] Migration file contains `pg_trgm` extension annotation and both GIN index definitions

## Key Decisions

- Address GIN index placed inside `OwnsOne` closure (on `location` builder) — required because `Address` is an owned-entity property; root builder would throw `InvalidOperationException`
- Migration folded into `Update2025` rather than a separate `AddTrigramIndexes` migration
- EF Core parameterizes the `term` value in `ILike` — no SQL injection risk despite `%term%` pattern construction
