---
phase: 03-security-hardening
plan: "03"
status: complete
completed_at: "2026-05-18"
requirements_closed:
  - SEC-03
  - SEC-05
subsystem: Gateway / ShopsService
tags: [rate-limiting, proxy-awareness, openapi, authorization]
dependency_graph:
  requires: []
  provides:
    - proxy-aware-rate-limit-partition-key
    - map-controller-allow-anonymous
  affects:
    - CoffeePeek.Gateway rate limiting effectiveness
    - CoffeePeek.ShopsService OpenAPI spec correctness
tech_stack:
  added: []
  patterns:
    - X-Forwarded-For header extraction with comma-split for multi-proxy chains
    - [AllowAnonymous] for intentionally public endpoints
key_files:
  modified:
    - CoffeePeek.Gateway/Extensions/RateLimitingExtensions.cs
    - CoffeePeek.ShopsService/Controllers/MapController.cs
decisions:
  - X-Forwarded-For leftmost IP used as primary partition key; Railway proxy injects real client IP here
  - RemoteIpAddress retained as third-priority fallback for non-proxy deployments
  - [AllowAnonymous] added without [EnableRateLimiting] — rate limiting for /api/map enforced at YARP layer
metrics:
  duration: "5m"
  tasks_completed: 2
  files_modified: 2
---

# Phase 3 Plan 03: Rate Limit Partition Key + MapController AllowAnonymous Summary

**One-liner:** Fixed GlobalLimiter to read X-Forwarded-For (real client IP behind Railway proxy) with three-source fallback chain; annotated MapController as [AllowAnonymous] for correct OpenAPI Bearer requirement omission.

## Objective

Fixed rate limiting partition key to use X-Forwarded-For (instead of RemoteIpAddress which is always the proxy's IP behind Railway). Added [AllowAnonymous] to MapController for correct OpenAPI semantics.

## Changes Made

- **CoffeePeek.Gateway/Extensions/RateLimitingExtensions.cs**: GlobalLimiter lambda now reads `X-Forwarded-For` (leftmost IP, comma-split) as primary partition key. Falls back to `X-Real-IP`, then `RemoteIpAddress`, then `"unknown"`. Updated comment documents Railway deployment assumption.
- **CoffeePeek.ShopsService/Controllers/MapController.cs**: Added `using Microsoft.AspNetCore.Authorization;` import and `[AllowAnonymous]` attribute on the class declaration. No [EnableRateLimiting] added — rate limiting is enforced at Gateway YARP layer.

## Verification

```
=== RemoteIpAddress (2 lines: 1 comment + 1 code fallback) ===
50:            // Fallback: X-Real-IP → RemoteIpAddress → "unknown". Split on comma handles multi-proxy chains.
56:                    ?? context.Connection.RemoteIpAddress?.ToString()

=== X-Forwarded-For (at least 1 line) ===
49:            // Partition by real client IP — Railway proxy injects X-Forwarded-For with real client IP as first entry.
54:                    context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()

=== X-Real-IP (at least 1 line) ===
50:            // Fallback: X-Real-IP → RemoteIpAddress → "unknown". Split on comma handles multi-proxy chains.
55:                    ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()

=== AllowAnonymous in MapController ===
15:[AllowAnonymous] // This endpoint is intentionally public — authentication not required

=== using Microsoft.AspNetCore.Authorization ===
4:using Microsoft.AspNetCore.Authorization;

=== EnableRateLimiting (expect 0) ===
0

=== dotnet build CoffeePeek.slnx --no-restore ===
Предупреждений: 30 (all NU1900 — NuGet vulnerability check network timeout, pre-existing)
Ошибок: 0
Build succeeded.
```

Note: `RemoteIpAddress` appears twice (once in the descriptive comment, once as the actual fallback code line). This is correct — the primary partition key is now X-Forwarded-For; RemoteIpAddress is the third-priority fallback only.

## Deviations

None — plan executed exactly as written.

## Self-Check: PASSED

- CoffeePeek.Gateway/Extensions/RateLimitingExtensions.cs: FOUND, X-Forwarded-For present
- CoffeePeek.ShopsService/Controllers/MapController.cs: FOUND, AllowAnonymous present
- Commit 7943dc9: FOUND
