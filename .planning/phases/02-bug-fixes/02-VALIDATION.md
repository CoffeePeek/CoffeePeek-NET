---
phase: 2
slug: bug-fixes
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-05-17
---

# Phase 2 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit v3 3.2.2 |
| **Config file** | none (xunit auto-discovered from csproj) |
| **Quick run command** | `dotnet test CoffeePeek.slnx --filter "FullyQualifiedName~BugFix"` |
| **Full suite command** | `dotnet test CoffeePeek.slnx` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test CoffeePeek.slnx --filter "FullyQualifiedName~BugFix"`
- **After every plan wave:** Run `dotnet test CoffeePeek.slnx`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 2-01-01 | 01 | 1 | BUG-01 | — | N/A | unit | `dotnet test --filter "CoffeeBean"` | ❌ W0 | ⬜ pending |
| 2-01-02 | 01 | 1 | BUG-02 | — | N/A | unit | `dotnet test --filter "SearchCoffeeShops"` | ❌ W0 | ⬜ pending |
| 2-02-01 | 02 | 1 | BUG-03 | T-2-01 | Non-owner receives ForbiddenException (403) | unit | `dotnet test --filter "DeleteReview"` | ❌ W0 | ⬜ pending |
| 2-02-02 | 02 | 1 | BUG-03 | T-2-01 | Owner can delete own review | unit | `dotnet test --filter "DeleteReview"` | ❌ W0 | ⬜ pending |
| 2-03-01 | 03 | 1 | BUG-04 | — | N/A | unit | `dotnet test --filter "GetCoffeeShop"` | ❌ W0 | ⬜ pending |
| 2-04-01 | 04 | 1 | BUG-05 | — | N/A | unit | `dotnet test --filter "CreateShopFromModeration"` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj` — new test project (mirrors `CoffeePeek.Account.Application.Tests`)
- [ ] `CoffeePeek.Shops.Application.Tests/GlobalUsings.cs` — global using imports
- [ ] `CoffeePeek.Shops.Application.Tests/Features/Review/DeleteReviewFromCoffeeShopHandlerTests.cs` — BUG-03 stubs
- [ ] `CoffeePeek.Shops.Application.Tests/Services/CreateShopFromModerationServiceTests.cs` — BUG-05 stubs
- [ ] Add project to `CoffeePeek.slnx` solution

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Cache invalidation actually clears beans from Redis | BUG-01 | Requires live Redis + Shops Service | Add a bean, call invalidation endpoint, verify bean list returns fresh data |
| `IsFavorite`/`IsVisited` correct in authenticated GET /api/CoffeeShops/{id} | BUG-04 | Requires live service + auth token | Authenticate, favorite a shop, GET the shop, verify IsFavorite=true |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
