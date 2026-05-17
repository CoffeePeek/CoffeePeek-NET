---
phase: 01-tech-debt-cleanup
plan: "01"
subsystem: shops-layer
tags: [tech-debt, bug-fix, exception-handling, domain]
dependency_graph:
  requires: []
  provides:
    - CoffeeShop constructor with Description parameter
    - ValidationException guards in UserFavoriteService (HTTP 400)
    - Deleted empty CoffeeShopRepository.cs
  affects:
    - CoffeePeek.Shops.Domain
    - CoffeePeek.Shops.Application
    - CoffeePeek.Shops.Infrastructure
tech_stack:
  added: []
  patterns:
    - CoffeeShop constructor extended to include Description — eliminates post-construction mutation
    - ValidationException used for guard-clause validation errors (maps to HTTP 400 via GlobalExceptionHandler)
key_files:
  created: []
  modified:
    - CoffeePeek.Shops.Domain/Aggregates/CoffeeShopAggregate/CoffeeShop.cs
    - CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs
    - CoffeePeek.Shops.Application/Services/UserFavoriteService.cs
  deleted:
    - CoffeePeek.Shops.Infrastructure/Services/CoffeeShopRepository.cs
decisions:
  - "Add description param to CoffeeShop constructor rather than keeping UpdateDetails call — eliminates post-construction mutation and sets each field exactly once"
  - "ValidationException chosen over ArgumentException for empty-Guid guards — maps to HTTP 400 via existing GlobalExceptionHandler, consistent with other service guards"
  - "CoffeeShopRepository.cs deleted — 1-line file with no references or implementation"
metrics:
  duration: "5 minutes"
  completed: "2026-05-17"
  tasks: 3
  files_changed: 4
---

# Phase 01 Plan 01: Shops Layer Tech Debt Cleanup Summary

Three independent Shops-layer tech debt items resolved: constructor redundancy eliminated, exception type corrected for proper HTTP status codes, empty dead file removed.

## Tasks Completed

| Task | Description | Commit |
|------|-------------|--------|
| 1 (TD-02) | Remove redundant UpdateDetails call in CreateShopFromModerationService | 4153785 |
| 2 (TD-04) | Replace InvalidOperationException with ValidationException in UserFavoriteService | 4153785 |
| 3 (TD-06) | Delete empty CoffeeShopRepository.cs | 4153785 |

## What Changed

### TD-02: Redundant UpdateDetails Call Removed

**File:** `CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs`
**Also:** `CoffeePeek.Shops.Domain/Aggregates/CoffeeShopAggregate/CoffeeShop.cs`

The `CoffeeShop` constructor set `Name` and `PriceRange`, then the very next line called `shop.UpdateDetails(shopDto.Name, shopDto.Description, (PriceRange)shopDto.PriceRange)` — re-setting `Name` and `PriceRange` while also setting `Description`.

Fix: Added `description` parameter to the `CoffeeShop(Guid, string, string?, PriceRange, Guid)` constructor so it sets all three fields at construction time. Removed the redundant `UpdateDetails` call entirely. Each of `Name`, `Description`, and `PriceRange` is now assigned exactly once.

### TD-04: InvalidOperationException → ValidationException

**File:** `CoffeePeek.Shops.Application/Services/UserFavoriteService.cs`

Four guard clauses (`userId == Guid.Empty`, `coffeeShopId == Guid.Empty` in both `AddToFavoritesAsync` and `RemoveFromFavoritesAsync`) threw `InvalidOperationException`. The `GlobalExceptionHandler` maps this to HTTP 500. These are input validation errors that should return HTTP 400.

Fix: All four replaced with `throw new ValidationException(...)` using identical message strings. `ValidationException` is in `CoffeePeek.Shared.Kernel.Exceptions` (already imported). `GlobalExceptionHandler` maps it to HTTP 400.

### TD-06: Empty CoffeeShopRepository.cs Deleted

**File:** `CoffeePeek.Shops.Infrastructure/Services/CoffeeShopRepository.cs`

The file contained exactly 1 line with no class body, no implementation, and no references anywhere in the codebase. Deleted with no impact on compilation or runtime.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Functionality] CoffeeShop constructor extended to accept Description**
- **Found during:** Task 1
- **Issue:** No standalone `SetDescription` method existed on `CoffeeShop`. `UpdateDetails` was the only way to set `Description`, but it also overwrote `Name` and `PriceRange`. Removing the call without another path to set `Description` would lose functionality.
- **Fix:** Added `description` parameter to constructor. This is a non-breaking internal change — the constructor is only called from one place (verified via grep).
- **Files modified:** `CoffeePeek.Shops.Domain/Aggregates/CoffeeShopAggregate/CoffeeShop.cs`
- **Commit:** 4153785

## Verification Results

| Check | Result |
|-------|--------|
| `grep -rn "InvalidOperationException" UserFavoriteService.cs` | no output (PASS) |
| `test ! -f CoffeeShopRepository.cs` | exits 0 (PASS) |
| `grep -n "UpdateDetails" CreateShopFromModerationService.cs` | no output (PASS) |
| `dotnet build CoffeePeek.Shops.Application` | 0 errors (PASS) |
| `dotnet build CoffeePeek.Shops.Infrastructure` | 0 errors (PASS) |

Note: Full solution build (`dotnet build CoffeePeek.slnx`) shows pre-existing NETSDK1226 errors on service host projects (missing .NET 10 prune package data for this SDK version). These are unrelated to this plan and existed before these changes.

## Known Stubs

None — all changes are correctness fixes with no stub patterns introduced.

## Threat Flags

None — no new network endpoints, auth paths, or schema changes introduced.

## Self-Check: PASSED

- `CoffeePeek.Shops.Domain/Aggregates/CoffeeShopAggregate/CoffeeShop.cs` — FOUND
- `CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs` — FOUND
- `CoffeePeek.Shops.Application/Services/UserFavoriteService.cs` — FOUND
- `CoffeePeek.Shops.Infrastructure/Services/CoffeeShopRepository.cs` — DELETED (confirmed)
- Commit 4153785 — FOUND in git log
