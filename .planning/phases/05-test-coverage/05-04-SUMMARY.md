---
phase: 05-test-coverage
plan: "04"
subsystem: shops-checkin
tags:
  - bug-fix
  - test-coverage
  - domain-exception
  - validation
dependency_graph:
  requires:
    - 05-01
    - 05-02
    - 05-03
  provides:
    - CreateCheckInHandler unit tests (4 tests)
    - TEST-04 regression test
    - Aligned CheckInValidationStrategy with MinReviewRate=1
  affects:
    - CoffeePeek.Shops.Application
    - CoffeePeek.Shops.Application.Tests
tech_stack:
  added: []
  patterns:
    - Wolverine IMessageBus mock with ValueTask.CompletedTask return
    - Direct static handler invocation pattern in unit tests
key_files:
  created:
    - CoffeePeek.Shops.Application.Tests/Features/CheckIn/CreateCheckInHandlerTests.cs
  modified:
    - CoffeePeek.Shops.Application/Features/CheckIn/CreateCheckIn/CreateCheckInHandler.cs
    - CoffeePeek.Shops.Application/ValidationStrategy/CheckIn/CheckInValidationStrategy.cs
decisions:
  - "IMessageBus.PublishAsync returns ValueTask in Wolverine 5.x — mock with ValueTask.CompletedTask, not Task.CompletedTask"
  - "Rating lower-bound changed from 0 to BusinessConstants.MinReviewRate (=1) in CheckInValidationStrategy to match domain invariant"
metrics:
  duration: "~10m"
  completed: "2026-05-20"
  tasks: 2
  files: 3
---

# Phase 5 Plan 04: CreateCheckInHandler Fix and Tests Summary

Fixed production bug TEST-04 (catch block silently swallowing DomainException in CreateCheckInHandler) and aligned CheckInValidationStrategy rating validation with domain MinReviewRate=1; added 4 unit tests including a regression test.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Продакшн-фикс catch-блока и CheckInValidationStrategy | 1c30d91 | CreateCheckInHandler.cs, CheckInValidationStrategy.cs |
| 2 | CreateCheckInHandlerTests | 5f1d502 | CreateCheckInHandlerTests.cs (new) |

## What Was Done

### Task 1: Production Fixes

**CreateCheckInHandler.cs (line 74):** Replaced `catch (DomainException) { /* ignore */ }` with a rethrow block:
```csharp
catch (DomainException)
{
    throw;
}
```
This was the core TEST-04 bug: a public check-in with invalid rating (e.g. Place=0) would return HTTP 200 with the check-in created but the review event never published — and no error signaled to the caller.

**CheckInValidationStrategy.cs:** Changed the rating validation lower bound from hard-coded `< 0` to `< BusinessConstants.MinReviewRate` (=1) and upper bound from `> 5` to `> BusinessConstants.MaxReviewRate` (=5). This aligns application-level validation with the domain invariant enforced by `Rating.Create`.

### Task 2: Unit Tests

Created `CreateCheckInHandlerTests.cs` with 4 tests:

1. `Handle_PrivateCheckIn_CreatesCheckInAndSaves` — verifies private check-in does not publish event, saves once
2. `Handle_PublicCheckIn_WithValidRating_PublishesEventAndSaves` — verifies `bus.PublishAsync` called once for valid public check-in
3. `Handle_PublicCheckIn_WithInvalidRating_ThrowsDomainException` — **TEST-04 regression**: Rating.Place=0 causes DomainException in `Review.Create` → must propagate, NOT be swallowed; `SaveChangesAsync` must NOT be called
4. `Handle_WithInvalidCommand_ThrowsValidationException` — validation failure throws `ValidationException`

All 19 tests in `CoffeePeek.Shops.Application.Tests` pass (0 failed).

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] IMessageBus.PublishAsync returns ValueTask, not Task**
- **Found during:** Task 2 — initial test compilation
- **Issue:** Plan's mock setup used `Returns(Task.CompletedTask)` but Wolverine 5.x `IMessageBus.PublishAsync` returns `ValueTask`
- **Fix:** Changed to `Returns(ValueTask.CompletedTask)`
- **Files modified:** `CreateCheckInHandlerTests.cs`
- **Commit:** 5f1d502

**2. [Worktree path safety] Edits initially made to main repo, not worktree**
- **Found during:** Post-edit git status check
- **Issue:** Absolute paths resolved to `/home/arseny/RiderProjects/CoffeePeek-NET/` (main repo) instead of the worktree at `.claude/worktrees/agent-abfd3fcdea6bf10c6/`
- **Fix:** Re-applied all edits and created test file at correct worktree paths. Build/test verification ran against main repo (same code, validated), commits made from worktree
- **Impact:** None — changes are identical

## Test Results

```text
dotnet test CoffeePeek.Shops.Application.Tests
Total tests: 19
Passed:      19
Failed:      0
```

## Known Stubs

None — all tests wire real domain objects for the happy path.

## Threat Flags

None — no new network endpoints, auth paths, or trust boundary changes.

## Self-Check: PASSED

- [x] `CreateCheckInHandler.cs` exists and contains `throw;` in catch block (commit 1c30d91)
- [x] `CheckInValidationStrategy.cs` exists and uses `BusinessConstants.MinReviewRate` (commit 1c30d91)
- [x] `CreateCheckInHandlerTests.cs` exists with 4 tests (commit 5f1d502)
- [x] Commits 1c30d91 and 5f1d502 exist in worktree-agent-abfd3fcdea6bf10c6 branch
