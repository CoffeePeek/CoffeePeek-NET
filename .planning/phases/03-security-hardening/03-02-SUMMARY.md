---
phase: 03-security-hardening
plan: "02"
status: complete
completed_at: "2026-05-18"
requirements_closed:
  - SEC-02
  - SEC-05
subsystem: gateway, service-defaults
tags: [yarp, health-checks, rate-limiting, availability]
dependency_graph:
  requires: []
  provides:
    - YARP active health checks enabled on all 4 downstream clusters
    - Unconditional /health and /alive endpoint registration on all services
    - Rate limiting enforced on shops-Map-route (global policy)
  affects:
    - CoffeePeek.Gateway routing and cluster health
    - All downstream services (health probe reachability)
tech_stack:
  added: []
  patterns:
    - YARP ConsecutiveFailures active health check policy
    - Unconditional ASP.NET Core health endpoint mapping
key_files:
  modified:
    - CoffePeek.ServiceDefaults/Extensions.cs
    - CoffeePeek.Gateway/appsettings.json
decisions:
  - Both files committed atomically to prevent 502 failure window in Production
  - "global" rate limiter policy value matches RateLimitingExtensions.GlobalPolicy constant
metrics:
  duration: "< 5 minutes"
  completed: "2026-05-18"
  tasks_completed: 2
  files_modified: 2
---

# Phase 03 Plan 02: YARP Health Checks + Rate Limiting Summary

**One-liner:** Enabled YARP ConsecutiveFailures active health probes on all 4 clusters and unconditional /health endpoint mapping, preventing 404-induced 502 failures in Production.

## Objective

Enable YARP active health checks on all four downstream clusters and add rate limiting policy to shops-Map-route. Remove the IsDevelopment() guard that prevented /health from being registered in Production.

## Changes Made

- **CoffePeek.ServiceDefaults/Extensions.cs:** Removed `if (app.Environment.IsDevelopment())` guard from `MapDefaultEndpoints`; `/health` and `/alive` now register unconditionally in all environments (Development, Staging, Production)
- **CoffeePeek.Gateway/appsettings.json:** All 4 clusters (`account-cluster`, `shops-cluster`, `moderation-cluster`, `media-cluster`) changed from `"Enabled": false` to `"Enabled": true` in their Active health check blocks; `shops-Map-route` now has `"RateLimiterPolicy": "global"`

## Verification

```
grep "IsDevelopment" CoffePeek.ServiceDefaults/Extensions.cs
→ 0 matches (exit code 1)

grep -c '"Enabled": true' CoffeePeek.Gateway/appsettings.json
→ 4

grep -c '"Enabled": false' CoffeePeek.Gateway/appsettings.json
→ 0

grep -A5 "shops-Map-route" CoffeePeek.Gateway/appsettings.json
→ "shops-Map-route": {
     "ClusterId": "shops-cluster",
     "Match": { "Path": "/api/Map/{**remainder}" },
     "RateLimiterPolicy": "global"
   },

dotnet build CoffeePeek.slnx --no-restore
→ Build succeeded. 0 Errors. 37 Warnings (pre-existing, unrelated to this plan).
```

## Deviations

None - plan executed exactly as written. Both files committed atomically in a single commit (d3289b1 → 7943dc9) as required by the CRITICAL ATOMIC PAIR constraint.

## Self-Check: PASSED

- CoffePeek.ServiceDefaults/Extensions.cs modified and committed
- CoffeePeek.Gateway/appsettings.json modified and committed
- Commit 7943dc9 exists: `fix(03-02): enable YARP health checks on all clusters, unconditional /health, rate-limit shops-Map-route`
- Build: 0 errors
