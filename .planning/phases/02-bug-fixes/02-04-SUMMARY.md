---
phase: 02-bug-fixes
plan: 04
subsystem: shops-application
tags: [bug-fix, persistence, event-driven, data-loss, BUG-05]
dependency_graph:
  requires: [02-bug-fixes-02]
  provides: [BUG-05-fix, shops-application-tests-scaffold]
  affects: [CoffeePeek.Shops.Application, CoffeePeek.Shops.Application.Tests]
tech_stack:
  added: []
  patterns: [IUnitOfWork-after-Add, xUnit-v3-regression-tests, Moq-Times.Once-verification]
key_files:
  created:
    - CoffeePeek.Shops.Application.Tests/Services/CreateShopFromModerationServiceTests.cs
    - CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj
    - CoffeePeek.Shops.Application.Tests/GlobalUsings.cs
  modified:
    - CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs
    - CoffeePeek.slnx
decisions:
  - "IUnitOfWork injected before ILogger in constructor (domain deps grouped together, logger last)"
  - "SaveChangesAsync called immediately after Add, before LogInformation (Option B from RESEARCH.md)"
  - "Test project scaffold cherry-picked from plan 02-02 objects into this worktree as prerequisite"
metrics:
  duration: "~12 minutes"
  completed: "2026-05-18"
  tasks_completed: 2
  files_modified: 5
---

# Phase 2 Plan 04: BUG-05 CreateShopFromModerationService SaveChangesAsync Fix Summary

**One-liner:** Inject IUnitOfWork into CreateShopFromModerationService and call SaveChangesAsync after Add to prevent silent data loss in the moderation-to-shops approval pipeline.

## What Was Built

### Task 1: Service Fix (commit: 0575e0f)

Modified `CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs`:

- Added `using CoffeePeek.Shared.Kernel;` for `IUnitOfWork` resolution
- Inserted `IUnitOfWork unitOfWork` constructor parameter immediately before `ILogger<CreateShopFromModerationService> logger` (order: shopRepository, coffeeBeanRepository, equipmentRepository, roasterRepository, brewMethodRepository, **unitOfWork**, logger)
- Added `await unitOfWork.SaveChangesAsync(cancellationToken);` immediately after `shopRepository.Add(shop)` and before the `LogInformation` call
- `ICreateShopFromModerationService` interface unchanged
- Duplicate-ModerationId guard (`if (exists) throw new InvalidOperationException(...)`) preserved and still runs before Add/Save

### Task 2: Test Project Scaffold + Regression Tests (commits: c853774, 6a261ca)

Created `CoffeePeek.Shops.Application.Tests` project (prerequisite cherry-picked from plan 02-02 scaffolding):
- `CoffeePeek.Shops.Application.Tests.csproj` - xUnit v3, FluentAssertions, Moq, ProjectReference to Shops.Application
- `GlobalUsings.cs` - global using Xunit;
- Registered under /CoffeePeek/CoffeePeek.Shops/Tests/ in CoffeePeek.slnx

Created `CoffeePeek.Shops.Application.Tests/Services/CreateShopFromModerationServiceTests.cs`:

**Fact 1:** CreateShopFromApprovedEventAsync_WhenModerationIdIsNew_AddsShopAndSavesExactlyOnce
- Setups ExistsByModerationId to return false
- Asserts returned Guid is not empty
- Asserts _shopRepoMock.Verify(r => r.Add(It.IsAny<CoffeeShop>()), Times.Once)
- Asserts _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once) - BUG-05 regression lock

**Fact 2:** CreateShopFromApprovedEventAsync_WhenModerationIdAlreadyExists_ThrowsAndDoesNotAddOrSave
- Setups ExistsByModerationId to return true
- Asserts ThrowAsync<InvalidOperationException>()
- Asserts _shopRepoMock.Verify(r => r.Add(...), Times.Never)
- Asserts _unitOfWorkMock.Verify(u => u.SaveChangesAsync(...), Times.Never)

## Verification Results

| Command | Result |
|---------|--------|
| dotnet build CoffeePeek.Shops.Application project | Exit 0, 0 errors |
| dotnet build CoffeePeek.slnx -c Debug --nologo --verbosity quiet | Exit 0, 0 errors |
| dotnet test filter CreateShopFromModerationServiceTests --no-build | Exit 0, 2 passed |
| dotnet test CoffeePeek.slnx --nologo --verbosity quiet | Exit 0, 322 total (11+207+2+76+26), 0 failed |

## Commits

| Hash | Type | Description |
|------|------|-------------|
| 0575e0f | fix(02-04) | BUG-05 - inject IUnitOfWork and call SaveChangesAsync after Add in CreateShopFromModerationService |
| c853774 | feat(02-04) | scaffold CoffeePeek.Shops.Application.Tests xUnit v3 project |
| 6a261ca | test(02-04) | add BUG-05 regression coverage in CreateShopFromModerationServiceTests |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocker] Test project scaffold not yet in this worktree**
- **Found during:** Task 2 setup
- **Issue:** Plan 02-02 scaffolded the test project on a different agent worktree branch. This worktree (agent-a3cd5b8e625efe1fa) did not have the project files.
- **Fix:** Cherry-picked bfcdad6 and 7755d1f git objects (plan 02-02 scaffold commits) into this worktree working tree; committed as feat(02-04): scaffold CoffeePeek.Shops.Application.Tests to unblock test authoring.
- **Files modified:** CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj, GlobalUsings.cs, CoffeePeek.slnx
- **Commit:** c853774

**2. [Rule 1 - Bug] Missing System usings in test file (auto-fixed)**
- **Found during:** Task 2 first build
- **Issue:** Test project csproj does not enable ImplicitUsings, so System.Threading.Tasks.Task and System.Threading.CancellationToken required explicit using declarations.
- **Fix:** Added using System; using System.Threading; using System.Threading.Tasks; to test file.
- **Commit:** 6a261ca (incorporated)

**3. [Rule 1 - Bug] PriceRange.Medium does not exist (auto-fixed)**
- **Found during:** Task 2 second build
- **Issue:** Used PriceRange.Medium in CreateMinimalShopDto() helper; actual enum value is PriceRange.Moderate.
- **Fix:** Changed to CoffeePeek.Contract.Enums.PriceRange.Moderate (fully qualified to resolve ambiguity).
- **Commit:** 6a261ca (incorporated)

## BUG-05 Data Loss Path - Closed

Before: CreateShopFromModerationService.CreateShopFromApprovedEventAsync called shopRepository.Add(shop) which only added the entity to the EF Core change tracker. No SaveChangesAsync was called. When the Wolverine message handler scope ended, the DbContext was disposed without flushing, and the approved shop was silently dropped from PostgreSQL.

After: The service now injects IUnitOfWork via primary constructor and calls await unitOfWork.SaveChangesAsync(cancellationToken) immediately after shopRepository.Add(shop). Regression test CreateShopFromApprovedEventAsync_WhenModerationIdIsNew_AddsShopAndSavesExactlyOnce uses Times.Once Moq verification to prevent future regressions.

## Known Stubs

None - no placeholder or stub values in any created/modified files.

## Threat Flags

None - no new network endpoints, auth paths, file access patterns, or schema changes introduced.

## Self-Check: PASSED

- [x] CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs - verified with IUnitOfWork unitOfWork and await unitOfWork.SaveChangesAsync(cancellationToken)
- [x] CoffeePeek.Shops.Application.Tests/Services/CreateShopFromModerationServiceTests.cs - verified with both Facts
- [x] Commits 0575e0f, c853774, 6a261ca - verified in git log
- [x] Full solution build: 0 errors
- [x] Full solution test: 322 passed, 0 failed
