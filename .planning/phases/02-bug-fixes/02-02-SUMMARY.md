---
phase: 02-bug-fixes
plan: 02
subsystem: testing
tags: [xunit, test-scaffold, shops, dotnet]

requires: []
provides:
  - "Empty xUnit v3 test project CoffeePeek.Shops.Application.Tests registered in CoffeePeek.slnx"
  - "Test infrastructure for plans 02-03 and 02-04 to add Shops Application regression tests"
affects: [02-03, 02-04]

tech-stack:
  added: []
  patterns:
    - "Mirror Account.Application.Tests template: OutputType=Exe, 6 PackageReferences, no Version= attributes"

key-files:
  created:
    - CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj
    - CoffeePeek.Shops.Application.Tests/GlobalUsings.cs
  modified:
    - CoffeePeek.slnx

key-decisions:
  - "Mirror CoffeePeek.Account.Application.Tests.csproj exactly — same 6 packages, OutputType=Exe, no coverlet.collector, no ImplicitUsings"
  - "GlobalUsings.cs contains only 'global using Xunit;' — plans 02-03 and 02-04 may add further aliases"
  - "NETSDK1226 error on service host projects is pre-existing environment issue (dotnet workload restore needed); does not affect test project build/test exit codes"

patterns-established:
  - "New test projects for Shops.Application mirror Account.Application.Tests — same SDK, OutputType, and package set"

requirements-completed: []

duration: 1min
completed: 2026-05-17
---

# Phase 2 Plan 02: Shops.Application.Tests Scaffold Summary

**Empty xUnit v3 test project CoffeePeek.Shops.Application.Tests scaffolded and registered in the solution, mirroring Account.Application.Tests with 6 packages, OutputType=Exe, and no version overrides**

## Performance

- **Duration:** ~1 min
- **Started:** 2026-05-17T13:44:07Z
- **Completed:** 2026-05-17T13:45:00Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments

- Created CoffeePeek.Shops.Application.Tests.csproj mirroring Account.Application.Tests template exactly
- Created GlobalUsings.cs with `global using Xunit;` for terse test files
- Registered project under `/CoffeePeek/CoffeePeek.Shops/Tests/` in CoffeePeek.slnx alongside existing Domain.Tests entry

## Task Commits

1. **Task 1: Create csproj file** - `bfcdad6` (feat)
2. **Task 2: Add GlobalUsings.cs and register in slnx** - `7755d1f` (feat)

**Plan metadata:** *(this SUMMARY.md commit)*

## Files Created/Modified

- `CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj` — xUnit v3 test project: Microsoft.NET.Sdk, net10.0, OutputType=Exe, IsPackable=false, 6 PackageReferences, ProjectReference to Shops.Application
- `CoffeePeek.Shops.Application.Tests/GlobalUsings.cs` — single line: `global using Xunit;`
- `CoffeePeek.slnx` — added `<Project Path="CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj" />` inside `<Folder Name="/CoffeePeek/CoffeePeek.Shops/Tests/">`

## Verification Results

| Command | Scope | Exit Code |
|---------|-------|-----------|
| `dotnet restore` | CoffeePeek.Shops.Application.Tests.csproj | 0 |
| `dotnet build -c Debug` | CoffeePeek.Shops.Application.Tests.csproj | 0 (0 errors, warnings are pre-existing NU1506) |
| `dotnet test --no-build` | CoffeePeek.Shops.Application.Tests.csproj | 0 (0 tests, expected Wave-0 state) |
| `dotnet test` | CoffeePeek.slnx | 0 |

## Decisions Made

- Mirrored Account.Application.Tests.csproj exactly — no coverlet.collector, no ImplicitUsings, same 6 packages
- GlobalUsings.cs starts with `global using Xunit;` only — plans 02-03/02-04 may add type aliases as needed
- NETSDK1226 errors on ASP.NET Core service host projects (ShopsService, AccountService, etc.) are pre-existing environment issues requiring `dotnet workload restore`; they are not caused by this plan and do not affect test project execution

## Deviations from Plan

None — plan executed exactly as written.

## Issues Encountered

Pre-existing `NETSDK1226` errors on service host projects when running `dotnet restore CoffeePeek.slnx` (exits 1 on full solution restore). These errors appear on the same projects before and after this plan's changes — confirmed by testing Account.Application.Tests.csproj restore in isolation (exits 0). The full solution `dotnet test CoffeePeek.slnx` exits 0, and the new test project builds and tests cleanly in isolation. Cause: `dotnet workload restore` has not been run in this environment.

## User Setup Required

None — no external service configuration required.

## Next Phase Readiness

- Plans 02-03 (BUG-03 regression tests) and 02-04 (BUG-05 regression tests) can add test classes to `CoffeePeek.Shops.Application.Tests/` immediately
- No additional scaffolding required

---
*Phase: 02-bug-fixes*
*Completed: 2026-05-17*
