---
phase: 3
slug: security-hardening
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-05-18
---

# Phase 3 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit v3 (existing) |
| **Config file** | CoffeePeek.slnx |
| **Quick run command** | `dotnet test CoffeePeek.slnx --filter "Category=Security"` |
| **Full suite command** | `dotnet test CoffeePeek.slnx` |
| **Estimated runtime** | ~60 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build CoffeePeek.slnx --no-restore`
- **After every plan wave:** Run `dotnet test CoffeePeek.slnx`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 60 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Secure Behavior | Test Type | Automated Command | Status |
|---------|------|------|-------------|-----------------|-----------|-------------------|--------|
| 03-01-01 | 01 | 1 | SEC-01 | appsettings.json files have `SendDefaultPii: false` | grep assertion | `grep -r "SendDefaultPii" */appsettings.json` | ⬜ pending |
| 03-01-02 | 01 | 1 | SEC-02 | YARP health check `Enabled: true` on all 4 clusters | grep assertion | `grep -A2 "Active" CoffeePeek.Gateway/appsettings.json` | ⬜ pending |
| 03-02-01 | 02 | 1 | SEC-03 | Rate limiter reads X-Forwarded-For header | unit test | `dotnet test --filter "RateLimiting"` | ⬜ pending |
| 03-02-02 | 02 | 1 | SEC-04 | Yandex API key absent from request URL | code review | `grep -n "apikey" */Services/YandexGeocodingService.cs` | ⬜ pending |
| 03-02-03 | 02 | 1 | SEC-05 | MapController route has rate limiter policy in gateway | grep assertion | `grep "shops-Map-route" CoffeePeek.Gateway/appsettings.json` | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

No new test project needed — verification is grep-based config assertions and one unit test for the rate limiter partitioning function. Existing `dotnet test` infrastructure covers Phase 3.

*Existing infrastructure covers all phase requirements.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Yandex API key header delivery | SEC-04 | Yandex API header auth must be confirmed to work | After fix, call geocoding endpoint and verify response is 200 with geocoding result |
| YARP health check probes reach `/health` | SEC-02 | Requires running gateway + downstream services | Start via Aspire, check YARP metrics or Gateway logs for health probe activity |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 60s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
