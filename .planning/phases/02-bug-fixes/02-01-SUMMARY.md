---
phase: 02-bug-fixes
plan: "01"
subsystem: shops-cache,shops-search,shops-controller
tags: [bug-fix, cache, error-handling, personalization, regression-test]
dependency_graph:
  requires: []
  provides: [BUG-01-fix, BUG-02-fix, BUG-04-fix, BUG-01-regression-test]
  affects: [CoffeePeek.Shared.Domain, CoffeePeek.Shops.Application, CoffeePeek.ShopsService, CoffeePeek.Shops.Domain.Tests]
tech_stack:
  added: []
  patterns: [surgical-single-line-fix, regression-test-as-pure-string-assertion]
key_files:
  created:
    - CoffeePeek.Shops.Domain.Tests/CacheKey/CoffeeBeanCacheKeyTests.cs
  modified:
    - CoffeePeek.Shared.Domain/Interfaces/Infrastructure/CacheKey.cs
    - CoffeePeek.Shops.Application/Features/CoffeeShop/SearchCoffeeShops/SearchCoffeeShopsHandler.cs
    - CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs
decisions:
  - "Used GetUserId() (returns Guid?) not GetUserIdOrThrow() in GetCoffeeShop — action lacks [Authorize] so anonymous requests must be supported"
  - "NETSDK1226 (AllowMissingPrunePackageData) treated as pre-existing environment issue — affects all 6 web service projects, not introduced by this plan"
  - "LoginAsync_WithValidCredentials_RevokesAllExistingSessions test failure treated as pre-existing — caused by Phase 1 TD-05 fix, documented in deferred-items.md"
metrics:
  duration_seconds: 211
  completed_date: "2026-05-17"
  tasks_completed: 4
  tasks_total: 4
  files_changed: 4
---

# Phase 2 Plan 01: BUG-01 Cache Key Prefix, BUG-02 Error Message, BUG-04 User Context Summary

**One-liner:** Three surgical single-line bug fixes restoring correct Redis cache invalidation, descriptive search errors, and per-user shop personalization, locked in with a prefix-assertion regression test.

## What Was Done

### Task 1 — BUG-01: Fix CoffeeBean.ListPattern() cache key prefix

**File:** `CoffeePeek.Shared.Domain/Interfaces/Infrastructure/CacheKey.cs`
**Line changed:** 134

**Before:**
```csharp
public static string ListPattern() => "bean:list:*";
```

**After:**
```csharp
public static string ListPattern() => "coffeebean:list:*";
```

**Why:** The write key `ListAll().Key` is `"coffeebean:list:all"`. For Redis SCAN/KEYS-based cache invalidation to match the entry, the pattern must share the same leading token. With `"bean:list:*"`, every admin invalidation of the `dictionaries` category was silently a no-op for CoffeeBeans — stale catalog entries persisted indefinitely.

**Commit:** 85d3005

---

### Task 2 — BUG-02: Descriptive error message in SearchCoffeeShopsHandler

**File:** `CoffeePeek.Shops.Application/Features/CoffeeShop/SearchCoffeeShops/SearchCoffeeShopsHandler.cs`
**Line changed:** 37

**Before:**
```csharp
if (cachedResponse == null) return Response<GetCoffeeShopsResponse>.Error("Error");
```

**After:**
```csharp
if (cachedResponse == null) return Response<GetCoffeeShopsResponse>.Error("Failed to retrieve coffee shop search results");
```

**Why:** The empty `"Error"` string gave clients and log aggregators no actionable information. The new message identifies the subsystem (coffee shop search) and the failure mode (retrieval failed).

**Commit:** 54b8cca

---

### Task 3 — BUG-04: Pass userContext.GetUserId() into GetCoffeeShopQuery

**File:** `CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs`
**Line changed:** 97

**Before:**
```csharp
var query = new GetCoffeeShopQuery(id);
```

**After:**
```csharp
var query = new GetCoffeeShopQuery(id, userContext.GetUserId());
```

**Why:** `GetCoffeeShopHandler` already conditionally fetches `IsFavorite`/`IsVisited` when `query.UserId.HasValue` (lines 43-50). With the single-argument constructor call, `UserId` was always `null`, so authenticated users always saw `IsFavorite = false` and `IsVisited = false`. The fix threads the nullable user id (correct for an anonymous-friendly endpoint) through to the handler.

`GetUserId()` (returns `Guid?`) was chosen over `GetUserIdOrThrow()` because the `GetCoffeeShop` action has no `[Authorize]` attribute — anonymous requests must continue to work.

**Commit:** 254f2c5

---

### Task 4 — BUG-01 Regression Test

**File:** `CoffeePeek.Shops.Domain.Tests/CacheKey/CoffeeBeanCacheKeyTests.cs` (NEW)

**Test:** `CoffeeBeanCacheKeyTests.ListPattern_StartsWithSamePrefixAsListAllKey` [Fact]

**What it asserts:**
- `CacheKey.CoffeeBean.ListPattern()` starts with the same leading token as `CacheKey.CoffeeBean.ListAll().Key` (`"coffeebean"`)
- `CacheKey.CoffeeBean.ListPattern()` ends with `"*"` (confirms it is a Redis wildcard pattern, not a literal key)

**Why:** A pure string property test with no I/O or mocking. If the prefix ever drifts again (the exact bug that caused BUG-01), this test catches it immediately without requiring a running Redis instance.

**Commit:** a8b2dac

---

## Verification Commands and Results

| Command | Exit Code | Notes |
|---------|-----------|-------|
| `dotnet build CoffeePeek.Shared.Domain/CoffeePeek.Shared.Domain.csproj -c Debug --nologo --verbosity quiet` | 0 | Task 1 build gate — clean |
| `dotnet build CoffeePeek.Shops.Application/CoffeePeek.Shops.Application.csproj -c Debug --nologo --verbosity quiet` | 0 | Task 2 build gate — clean |
| `dotnet build CoffeePeek.ShopsService/CoffeePeek.ShopsService.csproj -c Debug --nologo --verbosity quiet -p:AllowMissingPrunePackageData=true` | 0 | Task 3 build gate — NETSDK1226 is pre-existing |
| `dotnet test CoffeePeek.Shops.Domain.Tests/... --filter "FullyQualifiedName~CoffeeBeanCacheKeyTests"` | 0 | 1 passed |
| `dotnet build CoffeePeek.slnx -c Debug --nologo --verbosity quiet -p:AllowMissingPrunePackageData=true` | 0 | Full solution — 0 errors |
| `dotnet test CoffeePeek.slnx -p:AllowMissingPrunePackageData=true` | — | 12+207+35 pass; 1 pre-existing failure (see Deferred Issues) |
| `dotnet test CoffeePeek.Shops.Domain.Tests/... --nologo` | 0 | 12 passed (was 11) |

---

## Deviations from Plan

None — plan executed exactly as written. All three production fixes were single-line edits. The regression test file was created as specified.

---

## Deferred Issues

### Pre-existing Test Failure (out of scope)

**Test:** `CoffeePeek.Account.Application.Tests.Features.Auth.AuthServiceTests.AuthServiceTests.LoginAsync_WithValidCredentials_RevokesAllExistingSessions`

**Status:** FAILING — pre-existing since Phase 1 commit `80e3ac2` (TD-05 fix)

**Cause:** Phase 1 TD-05 intentionally removed `RevokeAllSessions()` from `AuthService.LoginAsync`. The unit test still asserts the old (incorrect) behavior. The test was not updated alongside the production change.

**Impact on this plan:** Zero — this test is in `CoffeePeek.Account.Application.Tests`, none of the files modified in this plan touch Account auth logic.

**Tracked in:** `.planning/phases/02-bug-fixes/deferred-items.md`

**Resolution:** Update the test in Phase 5 (Test Restoration) to assert `RevokeAllSessions()` is NOT called and that `AddSession` enforces session limits via `MaxActiveSessions`.

---

## Known Stubs

None — all three fixes wire real behavior. No placeholder values, TODO comments, or data stubs were introduced.

---

## Threat Surface Scan

No new network endpoints, auth paths, file access patterns, or schema changes were introduced. The controller change (`BUG-04`) passes an existing nullable user context value to an existing query — no new trust boundary crossing.

---

## Self-Check: PASSED

Files verified to exist:
- `/home/arseny/RiderProjects/CoffeePeek-NET/CoffeePeek.Shops.Domain.Tests/CacheKey/CoffeeBeanCacheKeyTests.cs` — FOUND
- `/home/arseny/RiderProjects/CoffeePeek-NET/CoffeePeek.Shared.Domain/Interfaces/Infrastructure/CacheKey.cs` — FOUND (contains `coffeebean:list:*`)
- `/home/arseny/RiderProjects/CoffeePeek-NET/CoffeePeek.Shops.Application/Features/CoffeeShop/SearchCoffeeShops/SearchCoffeeShopsHandler.cs` — FOUND (contains `Failed to retrieve coffee shop search results`)
- `/home/arseny/RiderProjects/CoffeePeek-NET/CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs` — FOUND (contains `new GetCoffeeShopQuery(id, userContext.GetUserId())`)

Commits verified:
- 85d3005 — BUG-01 CacheKey fix
- 54b8cca — BUG-02 error message fix
- 254f2c5 — BUG-04 user context fix
- a8b2dac — BUG-01 regression test
