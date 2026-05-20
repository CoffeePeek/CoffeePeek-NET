# Roadmap: CoffeePeek Tech Debt Resolution

## Milestone v1.0: Tech Debt Resolution

## Phases

- [x] **Phase 1: Tech Debt Cleanup** - Eliminate compile-time branching, hardcoded tokens, empty files, wrong exceptions, broken login flow (completed 2026-05-17)
- [x] **Phase 2: Bug Fixes** - Fix all known functional bugs — cache invalidation, ownership checks, personalization (completed 2026-05-18)
- [x] **Phase 3: Security Hardening** - Eliminate security vulnerabilities — Sentry PII, rate limiting, API key in URL, health checks (completed 2026-05-18)
- [x] **Phase 4: Performance Optimization** - Eliminate performance bottlenecks — correlated subquery, Redis KEYS, N+1 (completed 2026-05-20)
- [ ] **Phase 5: Test Coverage** - Close critical test gaps — Shops Application handlers, Shops Domain, regression tests

---

## Phase Details

### Phase 1: Tech Debt Cleanup
**Goal:** Eliminate technical debt — `#if DEBUG` branching, `CancellationToken.None`, empty files, wrong exceptions, broken login flow
**Depends on:** Nothing (first phase)
**Requirements:** TD-01, TD-02, TD-03, TD-04, TD-05, TD-06, TD-07
**Success Criteria** (what must be TRUE):
  1. All `#if DEBUG` persistence/CORS/Wolverine blocks replaced with runtime config — Release build works without special env vars
  2. `UserFavoriteService` returns HTTP 400 (not 500) on empty UserId
  3. `AuthService.LoginAsync` does not revoke all sessions when logging in from a new device
  4. No occurrences of `CancellationToken.None` remain in production code
**Plans:** 4/4 plans complete

Plans:
- [x] 01-01-PLAN.md — Remove redundant UpdateDetails, replace InvalidOperationException with ValidationException, delete empty CoffeeShopRepository (TD-02, TD-04, TD-06)
- [x] 01-02-PLAN.md — Remove RevokeAllSessions from login, thread CancellationToken in AuthService and UserNameChangedHandler (TD-03, TD-05)
- [x] 01-03-PLAN.md — Replace #if DEBUG in shared libraries: WolverineModule, CorsModule, GlobalExceptionHandler (TD-07)
- [x] 01-04-PLAN.md — Replace #if DEBUG in all four persistence DI files (TD-01)

### Phase 2: Bug Fixes
**Goal:** Fix all known functional bugs — cache invalidation, ownership checks, personalization
**Depends on:** Phase 1
**Requirements:** BUG-01, BUG-02, BUG-03, BUG-04, BUG-05
**Success Criteria** (what must be TRUE):
  1. `DELETE /api/CoffeeShopReviews/{id}` returns 403 if reviewer != requester
  2. Authenticated user sees correct IsFavorite/IsVisited in `GET /api/CoffeeShops/{id}`
  3. Bean catalog cache is invalidated after changes (cache key patterns match)
  4. `CoffeeShopApprovedEventHandler` explicitly calls `SaveChangesAsync`
**Plans:** 4/4 plans complete

**Wave 1** *(parallel — disjoint files)*
- [x] 02-01-PLAN.md — Surgical single-line fixes: CoffeeBean cache pattern prefix, descriptive search error message, thread userId into GetCoffeeShop, BUG-01 regression test (BUG-01, BUG-02, BUG-04)
- [x] 02-02-PLAN.md — Wave 0 scaffold: create CoffeePeek.Shops.Application.Tests project and register in CoffeePeek.slnx

**Wave 2** *(parallel — disjoint files)*
- [x] 02-03-PLAN.md — Add ForbiddenException, ownership check in DeleteReviewFromCoffeeShopHandler, controller wiring, regression tests (BUG-03)
- [x] 02-04-PLAN.md — Inject IUnitOfWork into CreateShopFromModerationService and call SaveChangesAsync after Add, regression tests (BUG-05)

**Cross-cutting constraints:**
- `CoffeePeek.Shops.Application.Tests` project must exist before Wave 2 test files compile
- Plans 02-03 and 02-04 both `depends_on: [02-bug-fixes-02]`

### Phase 3: Security Hardening
**Goal:** Eliminate security vulnerabilities — Sentry PII, rate limiting, API key in URL, health checks
**Depends on:** Phase 2
**Requirements:** SEC-01, SEC-02, SEC-03, SEC-04, SEC-05
**Success Criteria** (what must be TRUE):
  1. Committed appsettings.json files contain `SendDefaultPii: false` across all services
  2. Gateway rate limiting uses X-Forwarded-For, not RemoteIpAddress
  3. Yandex API key does not appear in request URLs (headers only)
  4. YARP active health checks enabled with ConsecutiveFailures policy on all clusters
**Plans:** 4/4 plans complete

**Wave 1** *(all parallel — disjoint files)*
- [x] 03-01-PLAN.md — Fix Sentry PII config: SendDefaultPii=false, MaxRequestBodySize=None in Account, Shops, Moderation appsettings.json (SEC-01)
- [x] 03-02-PLAN.md — Enable YARP health checks (Enabled:true on 4 clusters) + remove IsDevelopment() guard from ServiceDefaults + add RateLimiterPolicy to shops-Map-route (SEC-02, SEC-05)
- [x] 03-03-PLAN.md — Fix rate limiting partition key to X-Forwarded-For in Gateway; add [AllowAnonymous] to MapController (SEC-03, SEC-05)
- [x] 03-04-PLAN.md — Move Yandex API key from URL to DefaultRequestHeaders in Moderation Infrastructure (SEC-04)

### Phase 4: Performance Optimization
**Goal:** Eliminate performance bottlenecks — correlated subquery, Redis KEYS, N+1
**Depends on:** Phase 3
**Requirements:** PERF-01, PERF-02, PERF-03, PERF-04, PERF-05
**Success Criteria** (what must be TRUE):
  1. MinRating filter does not generate a correlated subquery (verified via EXPLAIN ANALYZE)
  2. Name/Address search uses an index (no full sequential scan)
  3. `RedisService.RemoveByPattern` uses SCAN instead of KEYS
  4. `UserNameChangedHandler` executes one UPDATE instead of loading records into memory
**Plans:** 3/4 plans executed

**Wave 1** *(parallel — disjoint files)*
- [x] 04-01-PLAN.md — Rewrite MinRating filter as LINQ Join subquery; remove redundant Include from GetUserFavoriteCoffeeShops (PERF-01, PERF-03)
- [x] 04-02-PLAN.md — Replace GetByUserId loop with ExecuteUpdateAsync in UserNameChangedHandler (PERF-05)
- [x] 04-03-PLAN.md — Replace _server.Keys().ToArray() with KeysAsync + await foreach in RedisService (PERF-04)

**Wave 2** *(depends on Wave 1 green)*
- [x] 04-04-PLAN.md — Replace ToLower().Contains() with EF.Functions.ILike; add pg_trgm extension + GIN indexes; run migration (PERF-02)

### Phase 5: Test Coverage
**Goal:** Close critical test gaps — Shops Application handlers, Shops Domain, regression tests
**Depends on:** Phase 4
**Requirements:** TEST-01, TEST-02, TEST-03, TEST-04, TEST-05
**Success Criteria** (what must be TRUE):
  1. `CoffeePeek.Shops.Application.Tests` contains tests for all 6 handlers addressed in Phase 2
  2. `DeleteReviewFromCoffeeShop` regression test: non-owner receives 403
  3. `CreateCheckInHandler` test: invalid rating is not silently swallowed
  4. `dotnet test CoffeePeek.slnx` passes without errors
**Plans:** 0/5 plans complete

**Wave 1** *(parallel)*
- [ ] 05-01-PLAN.md — Shops Domain unit tests: Review.Create, Rating.Create, CheckIn.Create, CoffeeShop constructor (TEST-02)
- [ ] 05-02-PLAN.md — UpdateEmailRequestHandler tests (TEST-05)
- [ ] 05-03-PLAN.md — AddToFavoriteHandler + GetShopsInBoundsHandler tests (TEST-01 partial)

**Wave 2** *(parallel, depends on Wave 1)*
- [ ] 05-04-PLAN.md — Fix CreateCheckInHandler catch block (catch→throw) + CreateCheckInHandler tests (TEST-01, TEST-04)
- [ ] 05-05-PLAN.md — SearchCoffeeShopsHandler + GetCoffeeShopHandler tests with ICacheService mocking (TEST-01 final)

---

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Tech Debt Cleanup | 4/4 | Complete   | 2026-05-17 |
| 2. Bug Fixes | 4/4 | Complete   | 2026-05-18 |
| 3. Security Hardening | 4/4 | Complete   | 2026-05-19 |
| 4. Performance Optimization | 4/4 | Complete   | 2026-05-20 |
| 5. Test Coverage | 0/0 | Not started | - |

---

*Roadmap created: 2026-05-17*
