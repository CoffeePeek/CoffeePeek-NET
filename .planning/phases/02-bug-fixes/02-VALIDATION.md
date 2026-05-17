---
phase: 2
slug: bug-fixes
status: draft
nyquist_compliant: true
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
| 2-01-01 | 01 | 1 | BUG-01 | — | N/A | grep+build | `grep -nE 'ListPattern\(\)\s*=>\s*"coffeebean:list:\*"' CoffeePeek.Shared.Domain/Interfaces/Infrastructure/CacheKey.cs && dotnet build CoffeePeek.Shared.Domain/CoffeePeek.Shared.Domain.csproj -c Debug --nologo --verbosity quiet` | ✅ | ⬜ pending |
| 2-01-02 | 01 | 1 | BUG-02 | — | N/A | grep+build | `grep -nE 'Response<GetCoffeeShopsResponse>.Error\("Failed to retrieve coffee shop search results"\)' CoffeePeek.Shops.Application/Features/CoffeeShop/SearchCoffeeShops/SearchCoffeeShopsHandler.cs && dotnet build CoffeePeek.Shops.Application/CoffeePeek.Shops.Application.csproj -c Debug --nologo --verbosity quiet` | ✅ | ⬜ pending |
| 2-01-03 | 01 | 1 | BUG-04 | — | N/A | grep+build | `grep -nE 'new GetCoffeeShopQuery\(id, userContext\.GetUserId\(\)\)' CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs && dotnet build CoffeePeek.ShopsService/CoffeePeek.ShopsService.csproj -c Debug --nologo --verbosity quiet` | ✅ | ⬜ pending |
| 2-01-04 | 01 | 1 | BUG-01 | — | N/A | unit | `dotnet test CoffeePeek.Shops.Domain.Tests/CoffeePeek.Shops.Domain.Tests.csproj --filter "FullyQualifiedName~CoffeeBeanCacheKeyTests" --nologo --verbosity quiet` | ❌ W0 | ⬜ pending |
| 2-02-01 | 02 | 1 | — | — | N/A | build+test | `dotnet restore CoffeePeek.slnx --nologo --verbosity quiet && dotnet build CoffeePeek.slnx -c Debug --nologo --verbosity quiet && dotnet test CoffeePeek.slnx --nologo --verbosity quiet --no-build` | ❌ W0 | ⬜ pending |
| 2-03-01 | 03 | 2 | BUG-03 | T-02-01 | Non-owner receives ForbiddenException (403) | grep+test | `grep -q 'throw new ForbiddenException' CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopHandler.cs && dotnet test CoffeePeek.slnx --filter "FullyQualifiedName~DeleteReviewFromCoffeeShopHandlerTests" --nologo --verbosity quiet --no-build` | ❌ W0 | ⬜ pending |
| 2-04-01 | 04 | 2 | BUG-05 | — | N/A | grep+test | `grep -q 'SaveChangesAsync' CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs && dotnet test CoffeePeek.slnx --filter "FullyQualifiedName~CreateShopFromModerationServiceTests" --nologo --verbosity quiet --no-build` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Plan 02-02 creates the Wave-0 scaffold. Plans 02-03 and 02-04 add test classes inside it.

- [ ] `CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj` — new test project (mirrors `CoffeePeek.Account.Application.Tests`), created by Plan 02-02 Task 1
- [ ] `CoffeePeek.Shops.Application.Tests/GlobalUsings.cs` — `global using Xunit;` stub, created by Plan 02-02 Task 2
- [ ] Add project to `CoffeePeek.slnx` solution — done in Plan 02-02 Task 2

Note: Test class files (`DeleteReviewFromCoffeeShopHandlerTests.cs`, `CreateShopFromModerationServiceTests.cs`) are NOT part of Wave 0 — they are added by Plans 02-03 and 02-04 respectively.

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
