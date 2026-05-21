---
phase: 05-test-coverage
plan: "03"
subsystem: shops-application-tests
tags: [tests, shops, favorites, geosearch, unit-tests]
dependency_graph:
  requires: []
  provides: [AddToFavoriteHandlerTests, GetShopsInBoundsHandlerTests]
  affects: [CoffeePeek.Shops.Application.Tests]
tech_stack:
  added: []
  patterns: [xUnit v3, Moq, FluentAssertions, static handler direct call]
key_files:
  created:
    - CoffeePeek.Shops.Application.Tests/Features/Favorite/AddToFavoriteHandlerTests.cs
    - CoffeePeek.Shops.Application.Tests/Features/CoffeeShop/GetShopsInBoundsHandlerTests.cs
  modified: []
decisions:
  - "result.Data используется вместо result.EntityId для проверки возвращаемого Id — CreateEntityResponse<Guid>.Success(id) передаёт id как data, а entityId остаётся null"
  - "Используется псевдоним SharedValidationResult = CoffeePeek.Shared.Validation.ValidationResult для разрешения конфликта имён с System.ComponentModel.DataAnnotations.ValidationResult"
metrics:
  duration: "4m"
  completed: "2026-05-20T17:01:53Z"
  tasks_completed: 2
  files_created: 2
  files_modified: 0
---

# Phase 5 Plan 03: Shops Application Handler Tests (AddToFavorite + GetShopsInBounds) Summary

**One-liner:** Unit tests for AddToFavoriteHandler (validation + service + UoW) and GetShopsInBoundsHandler (ICoffeeShopQueries mock) with 4 new tests, all green.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | AddToFavoriteHandlerTests | e4188b5 | CoffeePeek.Shops.Application.Tests/Features/Favorite/AddToFavoriteHandlerTests.cs |
| 2 | GetShopsInBoundsHandlerTests | 0f8ca2d | CoffeePeek.Shops.Application.Tests/Features/CoffeeShop/GetShopsInBoundsHandlerTests.cs |

## Verification

`dotnet test CoffeePeek.Shops.Application.Tests` — 9 passed (5 existing + 4 new), 0 failed.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] result.EntityId is null — used result.Data instead**
- **Found during:** Task 1 (after test run)
- **Issue:** Test asserted `result.EntityId.Should().Be(expectedId)` but `CreateEntityResponse<Guid>.Success(id, message)` sets `id` as `Data`, not `EntityId`. The `EntityId` parameter is the third (optional) arg and defaults to null.
- **Fix:** Changed assertion to `result.Data.Should().Be(expectedId)`
- **Files modified:** AddToFavoriteHandlerTests.cs
- **Commit:** e4188b5

**2. [Rule 3 - Blocking] ValidationResult naming conflict**
- **Found during:** Task 1 (compilation)
- **Issue:** Both `System.ComponentModel.DataAnnotations.ValidationResult` and `CoffeePeek.Shared.Validation.ValidationResult` were ambiguous after adding `using System.ComponentModel.DataAnnotations`.
- **Fix:** Added `using SharedValidationResult = CoffeePeek.Shared.Validation.ValidationResult;` alias and used it in Setup calls.
- **Files modified:** AddToFavoriteHandlerTests.cs
- **Commit:** e4188b5

## Known Stubs

None — all tests use real handler logic with mocked dependencies. No hardcoded data or placeholders.

## Threat Flags

None — test-only files, no production surface introduced.

## Self-Check: PASSED

- [x] AddToFavoriteHandlerTests.cs created at expected path
- [x] GetShopsInBoundsHandlerTests.cs created at expected path
- [x] Commits e4188b5 and 0f8ca2d exist
- [x] All 9 tests pass: `dotnet test CoffeePeek.Shops.Application.Tests` → 0 failed
