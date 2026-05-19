---
phase: 01-tech-debt-cleanup
plan: 02
subsystem: auth
tags: [cancellationtoken, session-management, wolverine, cqrs]

# Dependency graph
requires: []
provides:
  - AuthService.LoginAsync no longer revokes all sessions on every login
  - CancellationToken properly threaded through AuthService and UserNameChangedHandler
affects: [auth, shops]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Wolverine handlers accept CancellationToken as a named parameter; the framework injects it automatically"
    - "AddSession enforces MaxActiveSessions internally; RevokeAllSessions is reserved for security-breach paths only"

key-files:
  created: []
  modified:
    - CoffeePeek.Account.Application/Features/Auth/Login/AuthService.cs
    - CoffeePeek.Account.Application/Common/Interfaces/IAuthService.cs
    - CoffeePeek.Account.Application/Features/Auth/Login/LoginUserHandler.cs
    - CoffeePeek.Shops.Infrastructure/Consumers/UserNameChangedEventHandler.cs

key-decisions:
  - "TD-05: Removed user.RevokeAllSessions() from LoginAsync; AddSession already evicts the oldest session when MaxActiveSessions is reached, making the full revocation unnecessary and harmful to multi-device UX"
  - "TD-03: Added CancellationToken parameter with default=default to both LoginAsync and Handle; callers (LoginUserHandler, Wolverine dispatcher) pass their own CancellationToken, enabling proper request-lifecycle cancellation"

patterns-established:
  - "Pattern: RevokeAllSessions() is only appropriate in the security-breach detection path inside RotateRefreshToken, not in normal login flows"

requirements-completed:
  - TD-03
  - TD-05

# Metrics
duration: 8min
completed: 2026-05-17
---

# Phase 1 Plan 02: Auth/Token Tech Debt (TD-03, TD-05) Summary

**Removed silent full-session revocation on every login; threaded caller CancellationToken through AuthService and UserNameChangedHandler to enable proper DB-call cancellation under load**

## Performance

- **Duration:** ~8 min
- **Started:** 2026-05-17T00:00:00Z
- **Completed:** 2026-05-17T00:08:00Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- TD-05: `user.RevokeAllSessions()` removed from `AuthService.LoginAsync`. `AddSession` already enforces `MaxActiveSessions` by revoking the oldest token internally, so calling `RevokeAllSessions` was silently logging out all other devices on every login.
- TD-03: `LoginAsync` now accepts `CancellationToken cancellationToken = default`; `LoginUserHandler` passes the Wolverine-injected `ct` through; `IAuthService` interface signature updated to match.
- TD-03: `UserNameChangedHandler.Handle` now accepts `CancellationToken cancellationToken = default`; both `GetByUserId` and `SaveChangesAsync` receive the cancellation token instead of `CancellationToken.None`.

## Task Commits

Each task was committed atomically:

1. **Task 1 + Task 2: Remove RevokeAllSessions and thread CancellationToken** - `80e3ac2` (fix)

**Plan metadata:** (docs commit below)

## Files Created/Modified
- `CoffeePeek.Account.Application/Features/Auth/Login/AuthService.cs` - Removed `RevokeAllSessions()` call; added CancellationToken parameter; replaced `CancellationToken.None`
- `CoffeePeek.Account.Application/Common/Interfaces/IAuthService.cs` - Updated `LoginAsync` interface signature to include `CancellationToken`
- `CoffeePeek.Account.Application/Features/Auth/Login/LoginUserHandler.cs` - Passed `ct` to `authService.LoginAsync`
- `CoffeePeek.Shops.Infrastructure/Consumers/UserNameChangedEventHandler.cs` - Added `CancellationToken` parameter; replaced `CancellationToken.None` in `GetByUserId` and `SaveChangesAsync`

## Decisions Made
- `RevokeAllSessions()` is intentionally preserved in `User.cs` — it remains correct in the security-breach detection path inside `RotateRefreshToken` (when a reused token is detected, revoking all sessions is the right response).
- Both fixes committed in a single atomic commit because they are small, independent, and do not interleave.

## Deviations from Plan

None - plan executed exactly as written. The stale comment `// Session persistence (RevokeAllSessions + AddSession) is managed by Wolverine's transaction` in `LoginUserHandler.cs` was left untouched as it is out of scope for this plan.

## Issues Encountered
- `dotnet build CoffeePeek.slnx` reports pre-existing NETSDK1226 errors on service host projects (missing .NET 10 PrunePackageData on this machine). These errors existed before any changes in this plan. The directly affected projects (`CoffeePeek.Account.Application`, `CoffeePeek.Shops.Infrastructure`) build with zero errors.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Auth and shops infrastructure are clean for plans 03 and 04.
- The stale comment in `LoginUserHandler.cs` referencing `RevokeAllSessions` can be cleaned up in a future plan.

---
*Phase: 01-tech-debt-cleanup*
*Completed: 2026-05-17*
