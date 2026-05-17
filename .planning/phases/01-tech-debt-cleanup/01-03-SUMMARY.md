---
phase: 01-tech-debt-cleanup
plan: 03
subsystem: infra
tags: [dotnet, aspnetcore, cors, wolverine, exception-handling, environment, middleware]

# Dependency graph
requires: []
provides:
  - Runtime IsDevelopment() checks replacing #if DEBUG compile-time branches in WolverineModule, GlobalExceptionHandler, CorsModule
  - CorsModule now accepts IHostEnvironment parameter for environment-aware CORS policy selection
affects: [shared-persistence, shared-web, gateway]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Use IHostEnvironment.IsDevelopment() for runtime environment branching instead of #if DEBUG preprocessor directives"
    - "Pass IHostEnvironment as explicit parameter to static extension methods that need environment awareness"

key-files:
  created: []
  modified:
    - CoffeePeek.Shared.Persistence/Extensions/WolverineModule.cs
    - CoffeePeek.Shared.Web/Extensions/CorsModule.cs
    - CoffeePeek.Shared.Web/Handlers/GlobalExceptionHandler.cs
    - CoffeePeek.Gateway/Program.cs

key-decisions:
  - "IHostEnvironment passed explicitly to CorsModule rather than resolved from IServiceProvider — keeps extension method pure and testable"
  - "Microsoft.Extensions.Hosting using added to WolverineModule.cs and CorsModule.cs for IsDevelopment() extension method"
  - "CorsModule signature changed from AddCorsModule(services) to AddCorsModule(services, environment) — single caller updated (Gateway Program.cs)"

patterns-established:
  - "Pattern: extension methods that need dev/prod branching accept IHostEnvironment as explicit parameter"

requirements-completed: [TD-07]

# Metrics
duration: 8min
completed: 2026-05-17
---

# Phase 1 Plan 03: TD-07 Runtime Environment Checks Summary

**Three shared library files converted from #if DEBUG compile-time branching to IHostEnvironment.IsDevelopment() runtime checks, enabling Release builds to work correctly in Aspire dev environments.**

## Performance

- **Duration:** 8 min
- **Started:** 2026-05-17T10:21:51Z
- **Completed:** 2026-05-17T10:30:00Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Removed all `#if DEBUG / #else / #endif` blocks from WolverineModule.cs, GlobalExceptionHandler.cs, and CorsModule.cs
- CorsModule now accepts `IHostEnvironment environment` as second parameter and selects localhost/LAN policy or strict ALLOWED_ORIGINS policy at runtime
- WolverineModule now sets `TypeLoadMode.Auto` only when `builder.Environment.IsDevelopment()` is true
- GlobalExceptionHandler now includes StackTrace/InnerException only when `environment.IsDevelopment()` is true
- Gateway Program.cs updated to pass `builder.Environment` to `AddCorsModule()`

## Task Commits

Each task was committed atomically:

1. **Task 1 & 2: Replace #if DEBUG with runtime IsDevelopment() checks in all three files + update Gateway caller** - `7ec5a07` (refactor)

**Plan metadata:** (pending docs commit)

## Files Created/Modified
- `CoffeePeek.Shared.Persistence/Extensions/WolverineModule.cs` - Added `using Microsoft.Extensions.Hosting;`, replaced `#if DEBUG` TypeLoadMode block with `if (builder.Environment.IsDevelopment())`
- `CoffeePeek.Shared.Web/Handlers/GlobalExceptionHandler.cs` - Replaced `#if DEBUG` errorResponse block with `if (environment.IsDevelopment())` runtime check
- `CoffeePeek.Shared.Web/Extensions/CorsModule.cs` - Added `using Microsoft.Extensions.Hosting;`, added `IHostEnvironment environment` parameter, replaced `#if DEBUG` CORS policy block with `if (environment.IsDevelopment())`
- `CoffeePeek.Gateway/Program.cs` - Updated `AddCorsModule()` call to `AddCorsModule(builder.Environment)`

## Decisions Made
- `IHostEnvironment` passed as explicit parameter to `CorsModule.AddCorsModule()` rather than resolved from DI — this keeps the extension method signature clear and avoids requiring `IServiceProvider` access.
- Both tasks combined into a single commit since they are part of the same TD-07 concern and were all changed atomically.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- `dotnet build CoffeePeek.slnx` produces pre-existing NETSDK1226 errors ("Prune package data not found for .NETCoreApp 10.0 Microsoft.AspNetCore.App") in all service projects (`*.Web` SDK projects) — these are unrelated to this plan's changes and exist in the main repo as well. The three modified shared library projects (`Shared.Persistence`, `Shared.Web`) both build successfully with zero errors when built individually.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- TD-07 complete: all `#if DEBUG` compile-time branches in shared libraries have been eliminated
- The same binary can now be deployed to both development (Aspire, `ASPNETCORE_ENVIRONMENT=Development`) and production environments without recompiling
- No further callers of `AddCorsModule` exist that need updating; only `CoffeePeek.Gateway/Program.cs` uses it

## Threat Flags

| Flag | File | Description |
|------|------|-------------|
| threat_flag: elevation-of-privilege | CoffeePeek.Shared.Web/Extensions/CorsModule.cs | T-03-01: IsDevelopment() gate for CORS localhost allowlist controlled by ASPNETCORE_ENVIRONMENT — production deployments must not set Development environment |
| threat_flag: information-disclosure | CoffeePeek.Shared.Web/Handlers/GlobalExceptionHandler.cs | T-03-02: Stack trace emission guarded by IsDevelopment() — same behavior as before, now runtime-controlled |

## Self-Check: PASSED

- FOUND: CoffeePeek.Shared.Persistence/Extensions/WolverineModule.cs
- FOUND: CoffeePeek.Shared.Web/Extensions/CorsModule.cs
- FOUND: CoffeePeek.Shared.Web/Handlers/GlobalExceptionHandler.cs
- FOUND: CoffeePeek.Gateway/Program.cs
- FOUND: commit 7ec5a07
- VERIFIED: zero preprocessor directives in all three target files
- VERIFIED: IsDevelopment() present in all three target files

---
*Phase: 01-tech-debt-cleanup*
*Completed: 2026-05-17*
