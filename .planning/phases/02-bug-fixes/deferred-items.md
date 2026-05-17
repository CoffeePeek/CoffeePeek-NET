# Phase 2 — Deferred Items

## Pre-existing test failure (out of scope for 02-01)

**Test:** `CoffeePeek.Account.Application.Tests.Features.Auth.AuthServiceTests.AuthServiceTests.LoginAsync_WithValidCredentials_RevokesAllExistingSessions`

**Status:** FAILING — pre-existing since Phase 1 TD-05 fix (commit 80e3ac2)

**Cause:** Phase 1 TD-05 removed `RevokeAllSessions()` from `AuthService.LoginAsync`. The test still asserts that `RevokeAllSessions()` was called, but that call was intentionally removed. The test was not updated alongside the production code change.

**Action needed:** Update the test in a future plan to assert that `RevokeAllSessions()` is NOT called during normal login, and that `AddSession` enforces `MaxActiveSessions` instead. This is tracked as a Phase 5 test restoration item.

**Not caused by:** Plan 02-01 changes (CacheKey.cs, SearchCoffeeShopsHandler.cs, CoffeeShopsController.cs).
