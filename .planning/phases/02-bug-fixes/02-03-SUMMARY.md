---
phase: 02-bug-fixes
plan: "03"
subsystem: Shops.Application / Shared.Kernel / ShopsService
tags: [bug-fix, security, idor, access-control, forbidden-exception]
dependency_graph:
  requires: [02-bug-fixes-02]
  provides: [BUG-03-fix]
  affects: [CoffeePeek.Shared.Kernel, CoffeePeek.Shops.Application, CoffeePeek.ShopsService, CoffeePeek.Shops.Application.Tests]
tech_stack:
  added: []
  patterns: [BaseException-subclass-HTTP-status, JsonIgnore-property-target, Wolverine-handler-ownership-guard]
key_files:
  created:
    - CoffeePeek.Shared.Kernel/Exceptions/ForbiddenException.cs
    - CoffeePeek.Shops.Application.Tests/Features/Review/DeleteReviewFromCoffeeShopHandlerTests.cs
    - CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj
    - CoffeePeek.Shops.Application.Tests/GlobalUsings.cs
  modified:
    - CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopCommand.cs
    - CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopHandler.cs
    - CoffeePeek.ShopsService/Controllers/CoffeeShopReviewsController.cs
    - CoffeePeek.slnx
decisions:
  - "ForbiddenException placed in Shared.Kernel to mirror UnauthorizedException exactly — same two-constructor pattern, same BaseException subclass, HTTP 403 via (int)HttpStatusCode.Forbidden"
  - "RequestingUserId uses [property: JsonIgnore] target (not bare [JsonIgnore]) — record positional parameters require property: target to annotate the generated property, not the constructor parameter"
  - "Controller uses GetUserIdOrThrow() (not GetUserId()) because CoffeeShopReviewsController is [Authorize]-decorated — no anonymous access"
  - "Test namespace suffix matches package structure even though it causes an ambiguity between the Review class and the Features.Review namespace — resolved with fully-qualified type references in test file"
metrics:
  duration: "~20 minutes"
  completed: "2026-05-18"
  tasks_completed: 2
  files_changed: 8
---

# Phase 02 Plan 03: BUG-03 — IDOR Fix for DeleteReviewFromCoffeeShop Summary

**One-liner:** Ownership check added to DeleteReviewFromCoffeeShopHandler via ForbiddenException (HTTP 403), [property: JsonIgnore] guard on command, and three regression Facts in Shops.Application.Tests.

## What Was Built

BUG-03 (IDOR — Insecure Direct Object Reference) is closed. Any authenticated user could previously delete any review by guessing or observing its ID. The fix adds an ownership check at the Wolverine handler boundary and introduces a dedicated `ForbiddenException` that maps to HTTP 403 via the existing `GlobalExceptionHandler`.

### Files Created

| File | Purpose |
|------|---------|
| `CoffeePeek.Shared.Kernel/Exceptions/ForbiddenException.cs` | BaseException subclass with HttpStatusCode.Forbidden (403); two constructors mirroring UnauthorizedException |
| `CoffeePeek.Shops.Application.Tests/Features/Review/DeleteReviewFromCoffeeShopHandlerTests.cs` | Three Facts: owner-success, non-owner-forbidden, not-found |
| `CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj` | Test project scaffold (carried from Wave-0 02-02 work, added to worktree) |
| `CoffeePeek.Shops.Application.Tests/GlobalUsings.cs` | global using Xunit (carried from Wave-0 02-02 work, added to worktree) |

### Files Modified

| File | Change |
|------|--------|
| `DeleteReviewFromCoffeeShopCommand.cs` | Added `[property: JsonIgnore] Guid RequestingUserId` as second positional parameter |
| `DeleteReviewFromCoffeeShopHandler.cs` | Inserted ownership guard after NotFoundException check, before SoftDelete() |
| `CoffeeShopReviewsController.cs` | Replaced `new DeleteReviewFromCoffeeShopCommand(reviewId)` with `new DeleteReviewFromCoffeeShopCommand(reviewId, userContext.GetUserIdOrThrow())` |
| `CoffeePeek.slnx` | Registered `CoffeePeek.Shops.Application.Tests` under `/CoffeePeek/CoffeePeek.Shops/Tests/` folder |

## Test Results

Three Facts in `DeleteReviewFromCoffeeShopHandlerTests`:

| Test Method | Result |
|-------------|--------|
| `Handle_WhenOwnerDeletesOwnReview_SoftDeletesAndSaves` | PASSED |
| `Handle_WhenNonOwnerAttemptsDelete_ThrowsForbiddenExceptionAndDoesNotSave` | PASSED |
| `Handle_WhenReviewNotFound_ThrowsNotFoundException` | PASSED |

Full test suite: **116 tests, 0 failed** (Shops.Application: 3, Shops.Domain: 11, Account.Application: 76, Account.Infrastructure: 26).

### Verification Commands

```bash
dotnet build CoffeePeek.slnx -c Debug --nologo --verbosity quiet  # exit 0
dotnet test CoffeePeek.slnx --nologo --verbosity minimal           # 116 passed, 0 failed
dotnet test --filter "FullyQualifiedName~DeleteReviewFromCoffeeShopHandlerTests" --no-build  # 3 passed
```

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Worktree missing Wave-0 test project scaffold**
- **Found during:** Task 2
- **Issue:** The worktree was spawned from commit `a2ad29a` (before plan 02-02 ran). The `CoffeePeek.Shops.Application.Tests` project (csproj, GlobalUsings.cs, solution registration) existed in the main repo's `dev` branch but was absent from the worktree's working tree.
- **Fix:** Copied `CoffeePeek.Shops.Application.Tests.csproj` and `GlobalUsings.cs` from the main repo into the worktree, and added the project to the worktree's `CoffeePeek.slnx`. Both files are included in the task commit.
- **Files added:** `CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj`, `CoffeePeek.Shops.Application.Tests/GlobalUsings.cs`, `CoffeePeek.slnx`
- **Commit:** 17230f1

**2. [Rule 3 - Blocking] Missing System/Threading/Task usings in test file**
- **Found during:** Task 2, first build attempt
- **Issue:** The test project's `.csproj` does not have `<ImplicitUsings>enable</ImplicitUsings>`, so `Guid`, `Task`, `CancellationToken` were unresolved.
- **Fix:** Added explicit `using System;`, `using System.Threading;`, `using System.Threading.Tasks;` to the test file. Also resolved the `Review` type ambiguity (the class `Review` vs the namespace segment `Features.Review`) by using fully-qualified type references `Domain.Aggregates.ReviewAggregate.Review` within the test namespace.
- **Files modified:** `CoffeePeek.Shops.Application.Tests/Features/Review/DeleteReviewFromCoffeeShopHandlerTests.cs`
- **Commit:** 17230f1 (same commit, fixed before commit)

## Commits

| Hash | Message |
|------|---------|
| 17230f1 | fix(02-03): BUG-03 — add ownership check to DeleteReviewFromCoffeeShopHandler (IDOR) |

## Threat Surface Scan

No new network endpoints, auth paths, file access patterns, or schema changes introduced. The fix reduces the attack surface by adding a previously absent authorization check. No new threat flags.

## Known Stubs

None — all data flows are live production paths.

## Self-Check

- [ ] Check created files exist
- [ ] Check commits exist

## Self-Check: PASSED

All created and modified files verified present. Commit 17230f1 exists and contains 8 file changes. Three test Facts pass. Full solution builds with 0 errors.
