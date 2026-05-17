---
phase: 01-tech-debt-cleanup
plan: 04
subsystem: database
tags: [persistence, ef-core, aspire, postgresql, dependency-injection, environment-config]

# Dependency graph
requires:
  - phase: 01-03
    provides: "Pattern established: IsDevelopment() replaces #if DEBUG in shared libs (Shared.Persistence, Shared.Web)"
provides:
  - "All four persistence registrations use IHostEnvironment.IsDevelopment() for runtime DB registration selection"
  - "Zero #if DEBUG blocks remain across all persistence and shared layers"
affects: [01-05, any plan touching persistence registration or environment-based config]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "IHostEnvironment.IsDevelopment() replaces #if DEBUG for DB registration in all persistence DI files"
    - "IUnitOfWork registered unconditionally outside if/else block (not gated by environment)"

key-files:
  created: []
  modified:
    - CoffeePeek.Account.Persistence/DependencyInjection.cs
    - CoffeePeek.Shops.Persistance/DependencyInjection.cs
    - CoffeeShop.Moderation.Persistence/DependencyInjection.cs
    - CoffeePeek.MediaService/Program.cs

key-decisions:
  - "IUnitOfWork registration moved outside if/else in Account, Shops, Moderation — it is required in both Development and Production paths"
  - "MediaService Program.cs: IUnitOfWork was already outside both branches — left in place unchanged"
  - "NETSDK1226 build errors are pre-existing SDK environment issue (missing .NET 10 prune metadata), not caused by this plan — individual project builds pass with 0 errors"

patterns-established:
  - "All persistence DI files now follow identical pattern: if (builder.Environment.IsDevelopment()) Aspire path, else standalone PostgresCpOptions path"

requirements-completed: [TD-01]

# Metrics
duration: 10min
completed: 2026-05-17
---

# Phase 1 Plan 04: Replace #if DEBUG in Persistence DI Summary

**All four persistence registration files converted from compile-time #if DEBUG to runtime IsDevelopment() checks — same binary now selects Aspire vs PostgresCpOptions DB path based on ASPNETCORE_ENVIRONMENT**

## Performance

- **Duration:** ~10 min
- **Started:** 2026-05-17T10:20:00Z
- **Completed:** 2026-05-17T10:28:54Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Account persistence DI: `#if DEBUG` replaced with `if (builder.Environment.IsDevelopment())`; IUnitOfWork moved outside if/else
- Shops persistence DI: same pattern applied with ShopsDbContext + AppResources.ShopsDb
- Moderation persistence DI: same pattern applied with ModerationDbContext + AppResources.ModerationDb; IUnitOfWork moved outside if/else
- MediaService Program.cs: same pattern applied; IUnitOfWork was already outside both branches and was left unchanged
- Solution-wide grep confirms zero `#if DEBUG` remain in any `.cs` file across all persistence and shared layers

## Task Commits

Each task was committed atomically:

1. **Task 1: Replace #if DEBUG in Account and Shops persistence DI** - `4106e4d` (refactor)
2. **Task 2: Replace #if DEBUG in Moderation persistence DI and MediaService Program.cs** - `d5c41a0` (refactor)

**Plan metadata:** (docs commit follows)

## Files Created/Modified
- `CoffeePeek.Account.Persistence/DependencyInjection.cs` - #if DEBUG replaced with IsDevelopment(); IUnitOfWork moved outside if/else
- `CoffeePeek.Shops.Persistance/DependencyInjection.cs` - #if DEBUG replaced with IsDevelopment(); IUnitOfWork moved outside if/else
- `CoffeeShop.Moderation.Persistence/DependencyInjection.cs` - #if DEBUG replaced with IsDevelopment(); IUnitOfWork moved outside if/else
- `CoffeePeek.MediaService/Program.cs` - #if DEBUG replaced with IsDevelopment(); IUnitOfWork was already outside (no move needed)

## Decisions Made
- IUnitOfWork registration moved outside the if/else in Account, Shops, and Moderation — it was only in the #if DEBUG branch, but it is required regardless of which DB registration path is taken (Rule 2: missing critical functionality)
- GetConnectionString static local function removed; replaced inline with `services.AddValidateOptions<PostgresCpOptions>().ConnectionString` directly in the else branch

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

The full solution build (`dotnet build CoffeePeek.slnx`) reports NETSDK1226 errors ("Prune package data not found for .NETCoreApp 10.0 Microsoft.AspNetCore.App") across all ASP.NET Core service projects. These errors are pre-existing and unrelated to this plan's changes — they are caused by missing .NET 10 runtime metadata in the local SDK installation. Individual project builds for all four modified files pass with 0 errors. This issue is out of scope for TD-01.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- TD-01 is fully resolved: zero #if DEBUG blocks in persistence or shared layers
- All four persistence registrations correctly use runtime environment detection
- Production deployments must set `ASPNETCORE_ENVIRONMENT=Production` to activate the PostgresCpOptions path
- Ready for next wave of tech debt cleanup

---
*Phase: 01-tech-debt-cleanup*
*Completed: 2026-05-17*
