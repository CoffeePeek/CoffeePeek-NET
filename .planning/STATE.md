---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: Tech Debt Resolution
status: executing
stopped_at: Completed 03-04-PLAN.md — Yandex API key moved from URL to DefaultRequestHeaders (Phase 3 complete)
last_updated: "2026-05-18T19:25:00.000Z"
last_activity: 2026-05-18
progress:
  total_phases: 5
  completed_phases: 3
  total_plans: 12
  completed_plans: 12
  percent: 60
---

# Project State

**Project:** CoffeePeek Tech Debt Resolution
**Milestone:** v1.0 — Tech Debt Resolution
**Status:** In Progress
**Last Activity:** 2026-05-17

## Current Phase

Phase 3: Security Hardening — COMPLETE (4/4 plans done)

## Next Up

Phase 4: Performance Optimization
-> Run: /gsd:execute-phase 4

## Project Reference

See: .planning/PROJECT.md (updated 2026-05-17)

**Core value:** Every bug, vulnerability, and performance issue from CONCERNS.md is fixed and covered by a test.
**Current focus:** Phase 3 — Security Hardening

## Progress Bar

```
Phase 1 [##########] 100% COMPLETE
Phase 2 [##########] 100% COMPLETE
Phase 3 [##########] 100% COMPLETE
Phase 4 [..........] 0%
Phase 5 [..........] 0%
```

## Performance Metrics

| Metric | Value |
|--------|-------|
| Phases total | 5 |
| Phases complete | 2 |
| Requirements total | 27 |
| Requirements done | 12 |
| Phase 03-security-hardening P02 | 5m | 2 tasks | 2 files |
| Phase 03-security-hardening P03 | 5m | 2 tasks | 2 files |
| Phase 03-security-hardening P04 | 8m | 1 task | 2 files |

## Accumulated Context

### Key Decisions

- Order: Tech Debt -> Bugs -> Security -> Perf -> Tests — start with code cleanliness, then correctness, then safety
- Tests as a separate final phase — written after fixes to lock in correct behavior
- `#if DEBUG` -> runtime `IHostEnvironment.IsDevelopment()` — same compiled binary works in all environments
- Sentry PII -> false by default in committed config; override only in local dev
- `RevokeAllSessions()` method kept in User.cs — only removed from LoginAsync call site; still used in RotateRefreshToken security-breach path
- `IUnitOfWork` registration moved outside if/else in Account, Shops, Moderation persistence DI — it's needed in both Aspire and standalone paths
- BUG-01: CoffeeBean.ListPattern() must use "coffeebean:list:*" prefix to match write key for Redis cache invalidation
- BUG-02: SearchCoffeeShopsHandler returns descriptive message "Failed to retrieve coffee shop search results" (not empty "Error")
- BUG-04: GetCoffeeShop uses GetUserId() (nullable) not GetUserIdOrThrow() — endpoint is anonymous-friendly without [Authorize]
- SEC-03: X-Forwarded-For leftmost IP as GlobalLimiter partition key; RemoteIpAddress as fallback for non-proxy deployments
- SEC-05: [AllowAnonymous] on MapController without [EnableRateLimiting] — rate limiting enforced at YARP layer
- SEC-04: Option A — Yandex API key moved to X-Yandex-API-Key DefaultRequestHeaders; URL is a relative path with no apikey param

### Active Blockers

None

### Notes

- `CoffeePeek.Shops.*` contains the majority of critical issues
- `DeleteReviewFromCoffeeShop` is CRITICAL — any authenticated user can delete any review (BUG-03, Phase 2)
- `SendDefaultPii: true` is HIGH — passwords would be sent to Sentry if DSN is filled in (SEC-01, Phase 3)
- Naming typos (CoffePeek, Persistance, CoffeeShop.Moderation.*) are accepted as-is per CLAUDE.md

## Phase 3 — Completed 2026-05-18

All 5 SEC items resolved:

- SEC-01: SendDefaultPii=false + MaxRequestBodySize=None in Account, Shops, Moderation appsettings.json
- SEC-02: YARP health checks enabled (Enabled:true) on all 4 clusters + ConsecutiveFailures policy + removed IsDevelopment() guard
- SEC-03: X-Forwarded-For leftmost IP as GlobalLimiter partition key; RemoteIpAddress as fallback
- SEC-04: Yandex API key moved from URL query string to X-Yandex-API-Key DefaultRequestHeaders header
- SEC-05: [AllowAnonymous] on MapController; RateLimiterPolicy added to shops-Map-route

## Phase 2 — Completed 2026-05-18

All 5 BUG items resolved:

- BUG-01: CacheKey.CoffeeBean.ListPattern() → "coffeebean:list:*" (was "bean:list:*")
- BUG-02: SearchCoffeeShopsHandler error message → "Failed to retrieve coffee shop search results"
- BUG-03: DeleteReviewFromCoffeeShopHandler — ownership check + ForbiddenException (HTTP 403) + 3 regression tests
- BUG-04: CoffeeShopsController.GetCoffeeShop → new GetCoffeeShopQuery(id, userContext.GetUserId())
- BUG-05: CreateShopFromModerationService — IUnitOfWork injected, SaveChangesAsync called after Add + 2 regression tests

## Phase 1 — Completed 2026-05-17

All 7 TD items resolved:

- TD-01: #if DEBUG → IsDevelopment() in all 4 persistence DI files
- TD-02: Redundant UpdateDetails call removed from CreateShopFromModerationService (description added to constructor)
- TD-03: CancellationToken.None → caller token in AuthService.LoginAsync + UserNameChangedHandler.Handle
- TD-04: InvalidOperationException → ValidationException (HTTP 400 not 500) in UserFavoriteService
- TD-05: RevokeAllSessions() call removed from LoginAsync; AddSession enforces MaxActiveSessions
- TD-06: Empty CoffeeShopRepository.cs deleted
- TD-07: #if DEBUG → IsDevelopment() in WolverineModule, CorsModule, GlobalExceptionHandler

## Quick Tasks Completed

| Date       | Slug                               | Description                                               |
|------------|------------------------------------|-----------------------------------------------------------|
| 2026-05-19 | resend-email-confirmation-public   | POST /api/users/email-confirmation/resend — public endpoint, sends confirmation email by email address without auth |

## Session Continuity

Last session: 2026-05-18T19:25:00.000Z
Stopped at: Completed 03-04-PLAN.md — Yandex API key moved from URL to DefaultRequestHeaders (Phase 3 complete)
Resume: Begin Phase 4 — Performance Optimization.
