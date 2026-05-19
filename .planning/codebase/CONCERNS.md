# Codebase Concerns

**Analysis Date:** 2026-05-17

---

## Tech Debt

**`#if DEBUG` Persistence Branching:**
- Issue: Database registration uses compile-time `#if DEBUG` to switch between Aspire (`AddNpgsqlDbContext`) and standalone (`AddDatabase`) registration. This is a build-flag anti-pattern — the same binary cannot run both modes.
- Files: `CoffeePeek.Account.Persistence/DependencyInjection.cs`, `CoffeePeek.Shops.Persistance/DependencyInjection.cs`, `CoffeePeek.MediaService/Program.cs`, `CoffeeShop.Moderation.Persistence/DependencyInjection.cs`
- Impact: Release builds always run the manual `PostgresCpOptions` path; Aspire orchestration only works in DEBUG builds. Integration tests in Release configuration will fail silently if they expect Aspire-wired contexts.
- Fix approach: Use `IHostEnvironment` or a feature flag in `appsettings.json` to control the registration path at runtime, not compile time.

**Duplicate `UpdateDetails` Call in `CreateShopFromModerationService`:**
- Issue: `CoffeeShop` constructor already sets `Name` and `PriceRange`. The very next line calls `shop.UpdateDetails(shopDto.Name, shopDto.Description, shopDto.PriceRange)` which overwrites those values. The constructor call is redundant.
- Files: `CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs` lines 25-26
- Impact: Low — currently harmless because the same values are set twice, but it indicates incomplete refactoring and will confuse future changes.
- Fix approach: Remove the `UpdateDetails` call or remove the explicit property set from the constructor that takes `name` and `priceRange`.

**`CancellationToken.None` Hardcoded in Async Methods:**
- Issue: Two production methods pass `CancellationToken.None` instead of the caller-provided token.
- Files:
  - `CoffeePeek.Account.Application/Features/Auth/Login/AuthService.cs:19` — `GetByEmail(email, CancellationToken.None)`
  - `CoffeePeek.Shops.Infrastructure/Consumers/UserNameChangedEventHandler.cs:11` — `GetByUserId(message.UserId, CancellationToken.None)`
- Impact: These calls cannot be cancelled even if the upstream request is aborted, holding DB connections open unnecessarily under load.
- Fix approach: Thread the `CancellationToken` parameter through; both callers have access to one.

**`InvalidOperationException` Used Where Domain Exception Expected:**
- Issue: `UserFavoriteService` throws `InvalidOperationException` for guard checks instead of `DomainException` or `ValidationException` as used everywhere else.
- Files: `CoffeePeek.Shops.Application/Services/UserFavoriteService.cs` lines 18-25, 50-57
- Impact: `GlobalExceptionHandler` maps `InvalidOperationException` to HTTP 500, so a bad `UserId=Empty` input returns a 500 to the client instead of 400.
- Fix approach: Replace with `throw new ValidationException("UserId cannot be empty")`.

**Login Revokes All Sessions on Every Login:**
- Issue: `AuthService.LoginAsync` calls `user.RevokeAllSessions()` before adding the new session. This means a user who logs in from a phone revokes all other active device sessions silently.
- Files: `CoffeePeek.Account.Application/Features/Auth/Login/AuthService.cs:36`
- Impact: Surprise session termination for users with multiple devices; inconsistent with typical multi-device auth UX.
- Fix approach: Remove `RevokeAllSessions()` from login. Let `AddSession` enforce the `MaxActiveSessions` limit by revoking only the oldest session.

**`CoffeeShopRepository.cs` Is Empty:**
- Issue: `CoffeePeek.Shops.Infrastructure/Services/CoffeeShopRepository.cs` contains only 1 line (effectively blank).
- Files: `CoffeePeek.Shops.Infrastructure/Services/CoffeeShopRepository.cs`
- Impact: Suggests an incomplete implementation left over from an earlier refactoring pass.
- Fix approach: Either add the intended implementation or delete the file.

---

## Known Bugs

**Cache Key Mismatch for CoffeeBeans:**
- Symptoms: Invalidating the "beans" cache via `CacheInvalidationStrategy` removes `bean:list:*` keys, but the actual write uses key `coffeebean:list:all`. The patterns never overlap, so bean catalog cache is never invalidated.
- Files:
  - Key stored as: `CoffeePeek.Shared.Domain/Interfaces/Infrastructure/CacheKey.cs:129` — `"coffeebean:list:all"`
  - Pattern used for invalidation: `CacheKey.cs:134` — `"bean:list:*"` and `CoffeePeek.Shared.Persistence/Cache/CacheInvalidationStrategy.cs:25` — `"bean:list:*"`
- Trigger: Any call that stores the bean catalog (e.g., `GetAllBeansHandler`) then an admin-triggered cache invalidation — stale data is served indefinitely.
- Fix: Change `CacheKey.CoffeeBean.ListPattern()` to return `"coffeebean:list:*"`, or change the key prefix to `"bean:list:all"`.

**`SearchCoffeeShopsHandler` Returns Generic `"Error"` on Cache Miss:**
- Symptoms: If Redis returns `null` from the factory (theoretically impossible but defensive code path), the handler returns `Response.Error("Error")` — a blank message that reaches the client as a 200 with `"Error"` in the body.
- Files: `CoffeePeek.Shops.Application/Features/CoffeeShop/SearchCoffeeShops/SearchCoffeeShopsHandler.cs:37`
- Trigger: Redis factory returning null (e.g., zero results from DB while factory path is hit).
- Fix: Replace with a more descriptive message or throw `DatabaseException`.

**`DeleteReviewFromCoffeeShop` Does Not Verify Ownership:**
- Symptoms: Any authenticated user can delete any review by ID. The handler fetches the review by `ReviewId` and soft-deletes it without checking that the requester is the review author or a moderator.
- Files: `CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopHandler.cs`, `CoffeePeek.ShopsService/Controllers/CoffeeShopReviewsController.cs:51`
- Trigger: Authenticated user calls `DELETE /api/CoffeeShopReviews/{reviewId}` with any valid review ID.
- Fix: Load the review, compare `review.UserId == requestingUserId`, throw `UnauthorizedException` if mismatched (or allow admins/moderators via role check).

**`GetCoffeeShopQuery` Ignores `UserId` in Cached Path:**
- Symptoms: `GetCoffeeShopHandler` stores the full shop DTO (including IsFavorite/IsVisited defaults) in cache. The cached entry is then personalised per-user by mutating `shopDto`. However, the cached object is a C# record `with` expression, so the original in Redis is NOT mutated — this is correct. But the `CoffeeShopsController.GetCoffeeShop` action does not pass `UserId` to the query, so logged-in users never see personalised `IsFavorite`/`IsVisited` on the detail endpoint.
- Files: `CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs:97-98`, `CoffeePeek.Shops.Application/Features/CoffeeShop/GetCoffeeShop/GetCoffeeShopQuery.cs`
- Trigger: Authenticated user calls `GET /api/CoffeeShops/{id}` — `IsFavorite` and `IsVisited` are always false.
- Fix: Populate `GetCoffeeShopQuery(id, userId: userContext.GetUserId())` in the controller action.

**`CoffeeShopApprovedEventHandler` Has No `SaveChangesAsync`:**
- Symptoms: `ModerationShopApproveHandler` calls `createShopService.CreateShopFromApprovedEventAsync()` which calls `shopRepository.Add(shop)` but the handler itself never calls `unitOfWork.SaveChangesAsync()`. Data is only persisted if Wolverine's auto-apply transaction commits the EF Core context.
- Files: `CoffeePeek.Shops.Infrastructure/Consumers/CoffeeShopApprovedEventHandler.cs`
- Impact: Relies entirely on Wolverine transaction auto-commit. If Wolverine policy is removed or the handler is moved outside the transaction, shops from moderation approvals are silently lost.
- Fix: Explicitly call `unitOfWork.SaveChangesAsync()` to make the persistence contract explicit and audit-safe.

---

## Security Considerations

**Sentry `SendDefaultPii: true` Committed to Source:**
- Risk: Sentry is configured with `"SendDefaultPii": true` and `"MaxRequestBodySize": "Always"` in checked-in `appsettings.json` files. If Sentry DSN is ever filled in, full request bodies (including passwords in login requests) and user PII are sent to Sentry.
- Files: `CoffeePeek.AccountService/appsettings.json:11-12`, `CoffeePeek.ModerationService/appsettings.json:11-12`, `CoffeePeek.ShopsService/appsettings.json:22-23`
- Current mitigation: Sentry DSN is empty in committed config.
- Recommendations: Change `SendDefaultPii` to `false` and `MaxRequestBodySize` to `"None"` or `"Medium"` in committed config. Override to fuller logging only in local developer config (`.gitignore`-d).

**All YARP Health Checks Disabled:**
- Risk: Every cluster in `appsettings.json` has `"Active": { "Enabled": false }`. The gateway routes requests to downstream services without health awareness; a dead service receives traffic and returns errors to users without fast-fail.
- Files: `CoffeePeek.Gateway/appsettings.json` — all four clusters (`account-cluster`, `shops-cluster`, `moderation-cluster`, `media-cluster`)
- Current mitigation: Kubernetes/Railway health routing at infrastructure level may handle this.
- Recommendations: Enable active health checks and set the `ConsecutiveFailures` policy so failed destinations are temporarily removed.

**Rate Limiting Uses Raw IP, No Proxy Header Consideration:**
- Risk: `RateLimitingExtensions` partitions by `context.Connection.RemoteIpAddress`. Behind a load balancer or Railway proxy, all traffic shares a single IP, defeating per-client limits entirely.
- Files: `CoffeePeek.Gateway/Extensions/RateLimitingExtensions.cs:52`
- Current mitigation: None.
- Recommendations: Use `X-Forwarded-For` or `X-Real-IP` header (with trusted proxy validation) as the partition key.

**Yandex Geocoding API Key Logged Via URL Construction:**
- Risk: `YandexGeocodingService` builds the URL as `$"{_options.BaseUrl}?apikey={_options.ApiKey}&geocode={encodedAddress}&format=json&results=1"`. If ASP.NET Core's request logging or any HTTP client diagnostic listener logs the outgoing request URL, the API key is written to logs.
- Files: `CoffeePeek.Moderation.Infrastructure/Services/YandexGeocodingService.cs:29`
- Current mitigation: No evidence of HTTP client request URL logging enabled, but default `HttpClient` diagnostics do log URLs in development.
- Recommendations: Pass the API key as a header or use a named `HttpClient` with the key added via `DefaultRequestHeaders`, removing it from the URL.

**Phone Number Validation Is Belarus-Only:**
- Risk: The `PhoneNumber` value object enforces the `+375` Belarusian format globally. Any user with a non-Belarusian number cannot register a phone number; this is a functional constraint that is not documented in any API contract or user-facing error.
- Files: `CoffeePeek.Account.Domain/Entities/UserAggregate/PhoneNumber.cs:41`
- Current mitigation: The error message mentions "Belarusian" format explicitly.
- Recommendations: Document this constraint in OpenAPI; or abstract validation to allow configurable country codes.

---

## Performance Bottlenecks

**MinRating Filter Uses Correlated Subquery Per Row:**
- Problem: The `MinRating` filter in `CoffeeShopQueries.Search` runs a correlated subquery (`context.Reviews.Where(r => r.CoffeeShopId == s.Id).GroupBy(...).Select(g => g.Average(...)).FirstOrDefault()`) inside the main `WHERE` clause — once per candidate shop row.
- Files: `CoffeePeek.Shops.Persistance/Queries/CoffeeShopQueries.cs:58-63`
- Cause: Translated to a correlated subquery in PostgreSQL (N subqueries for N shops). On a table with thousands of shops, this runs a full aggregation scan on the Reviews table per filtered shop.
- Improvement path: Pre-compute `AverageRating` as a denormalized column on `CoffeeShop`, updated via event on review creation/deletion. Or join against a pre-aggregated CTE.

**`ToLower().Contains()` Search Prevents Index Use:**
- Problem: Full-text search in `CoffeeShopQueries.Search` uses `s.Name.ToLower().Contains(term)` and `s.Location.Address.ToLower().Contains(term)`. EF Core translates this to `LOWER(Name) LIKE '%term%'` — a leading wildcard which cannot use a B-tree index.
- Files: `CoffeePeek.Shops.Persistance/Queries/CoffeeShopQueries.cs:20-21`
- Cause: No full-text search index exists; the query performs a full sequential scan.
- Improvement path: Add a PostgreSQL `tsvector` generated column with a GIN index and use `EF.Functions.ToTsVector` / `EF.Functions.FtsQuery`, or use `ILIKE` with a trigram index (`pg_trgm`).

**`GetUserFavoriteCoffeeShops` Uses `Include` With `AsSplitQuery` But No Explicit N+1 Control:**
- Problem: `CoffeeShopQueries.GetUserFavoriteCoffeeShops` uses `.Include(x => x.CoffeeShop).AsSplitQuery()` followed by `.Select(x => x.CoffeeShop).ProjectToType<CoffeeShopDetailsDto>()`. Including the navigation and then projecting means the included data is loaded but then projected out — the `Include` is wasted and adds overhead.
- Files: `CoffeePeek.Shops.Persistance/Queries/CoffeeShopQueries.cs:111-118`
- Improvement path: Remove the `Include` — `ProjectToType<CoffeeShopDetailsDto>()` with Mapster handles joins directly via the projection.

**`RemoveByPattern` Uses Redis `KEYS` (Blocking):**
- Problem: `RedisService.RemoveByPattern` calls `_server.Keys(pattern: pattern)` which issues the Redis `KEYS` command — a blocking O(N) operation over the entire keyspace.
- Files: `CoffeePeek.Shared.Persistence/Cache/Redis/RedisService.cs:188`
- Cause: Using `IServer.Keys()` (backed by `KEYS`) rather than `SCAN`.
- Improvement path: Replace with `_server.Keys()` using `pageSize` parameter (which uses `SCAN` under the hood in StackExchange.Redis >= 2.0), or use `SCAN` explicitly with `cursor` iteration.

**`UserNameChangedHandler` Loads All User Reviews Into Memory:**
- Problem: `UserNameChangedHandler` calls `reviewRepository.GetByUserId(message.UserId, CancellationToken.None)` which fetches all reviews for a user into memory before iterating and updating them.
- Files: `CoffeePeek.Shops.Infrastructure/Consumers/UserNameChangedEventHandler.cs:11-22`
- Cause: A user with many reviews would have all of them materialized in a single round-trip.
- Improvement path: Issue a single `UPDATE reviews SET user_name = $1 WHERE user_id = $2 AND is_soft_delete = false` via `ExecuteUpdateAsync`.

---

## Fragile Areas

**`#if DEBUG` Branching in Shared Persistence Library:**
- Files: `CoffeePeek.Shared.Persistence/Extensions/WolverineModule.cs:24`, `CoffeePeek.Shared.Web/Extensions/CorsModule.cs:18`, `CoffeePeek.Shared.Web/Handlers/GlobalExceptionHandler.cs:28`
- Why fragile: Shared libraries compiled with `#if DEBUG` cause behavior differences between Debug and Release builds across all consumers. CORS in debug allows any `localhost` or `*.local` origin; in release requires `ALLOWED_ORIGINS` env var (throws `InvalidOperationException` if absent).
- Safe modification: Test CORS configuration explicitly in Release before deploying. Do not rely on implicit DEBUG behavior.
- Test coverage: No tests for the CORS configuration path.

**`CreateCheckInHandler` Silently Swallows `DomainException` from Review Creation:**
- Files: `CoffeePeek.Shops.Application/Features/CheckIn/CreateCheckIn/CreateCheckInHandler.cs:74`
- Why fragile: When a public check-in is submitted with an invalid rating, `Review.Create` throws `DomainException`. This is caught and silently ignored (`/* ignore */`). The check-in is still saved, but no review event is published and no `ReviewId` is set on the response. The client receives a success response with no indication that the review was dropped.
- Safe modification: Either validate `Rating` before calling `Review.Create` (preferred — validation is already done in `CheckInValidationStrategy`), or propagate the exception to return a 400.
- Test coverage: No test exercises the silent-swallow path.

**Inconsistent Error Response Patterns in Shops Handlers:**
- Files:
  - `GetCoffeeShopHandler.cs:41` — returns `Response.Error("Shop not found")` with HTTP 200
  - `GetReviewByIdHandler.cs:20` — returns `Response.Error(HttpStatusCode.NotFound, "Review not found")` with HTTP 200
  - `RemoveFromFavoriteHandler.cs:19` — returns `UpdateEntityResponse.Error(...)` with HTTP 200
  - `DeleteReviewFromCoffeeShopHandler.cs` — throws `NotFoundException` which becomes HTTP 404
- Why fragile: Callers must check `IsSuccess` on the response body at HTTP 200 to detect failures in some handlers, but expect HTTP error codes in others. Client SDK code must special-case each endpoint.
- Safe modification: Standardize: either always throw typed exceptions (preferred — consistent with `GlobalExceptionHandler` mappings) or always return typed `Response<T>` with HTTP status properly set.

---

## Scaling Limits

**In-Memory `GetShopsInBounds` — Hardcoded Limit of 500:**
- Current capacity: Returns at most 500 shops per map viewport query with no pagination.
- Files: `CoffeePeek.Shops.Persistance/Queries/CoffeeShopQueries.cs:107`
- Limit: Dense urban map views could hit the cap, silently omitting shops beyond 500 results.
- Scaling path: Add viewport clustering on the DB side (PostGIS `ST_ClusterDBSCAN`) or return a lower limit with a "zoom in to see more" response flag.

**Redis Cache Keys Do Not Include Service Namespace:**
- Current capacity: `CacheKey` entries use flat prefixes like `shop:detail:*`, `user:profile:*`, and `auth:*`. All services share a single Redis instance.
- Files: `CoffeePeek.Shared.Domain/Interfaces/Infrastructure/CacheKey.cs`
- Limit: If a second service introduces a key like `user:profile:*` for a different domain concept, keys collide.
- Scaling path: Prefix all keys with a service name segment (e.g., `account:user:profile:*`, `shops:shop:detail:*`).

---

## Dependencies at Risk

**`Shared.Persistence` Wolverine Module Requires PostgreSQL for Outbox:**
- Risk: `opts.PersistMessagesWithPostgresql(...)` ties every service's message persistence to PostgreSQL. Switching any service away from PostgreSQL (e.g., to use a different store) requires forking `WolverineModule`.
- Impact: All services: `CoffeePeek.AccountService`, `CoffeePeek.ShopsService`, `CoffeePeek.ModerationService`, `CoffeePeek.MediaService`.
- Files: `CoffeePeek.Shared.Persistence/Extensions/WolverineModule.cs:38`
- Migration plan: Low urgency; document the coupling explicitly in ARCHITECTURE.md.

---

## Missing Critical Features

**No Authorization Scope on `GetShopsInBounds` (Map Endpoint):**
- Problem: `MapController` has no `[Authorize]` attribute. The endpoint is entirely public — no rate limiting is applied per-user, only per-IP. Combined with the global-limiter bypass issue (see rate limiting concern above), this endpoint can be hammered at scale with no per-client throttle.
- Files: `CoffeePeek.ShopsService/Controllers/MapController.cs`

**No Cache Invalidation When a Shop Is Approved From Moderation:**
- Problem: When `ModerationShopApproveHandler` creates a new shop, there is no cache invalidation for any `shop:list:*` or `shop:search:*` keys. New shops will not appear in search or city list results until the TTL expires (5 minutes for search, 5 minutes for city lists).
- Files: `CoffeePeek.Shops.Infrastructure/Consumers/CoffeeShopApprovedEventHandler.cs`
- Blocks: Newly approved shops invisible to users for up to 5 minutes.

**`MediaService` Has No Authorization on Shop Photo Upload:**
- Problem: `PhotosController.GenerateUploadUrls` (shop photo endpoint) only checks `userContext.GetUserIdOrThrow()` — any authenticated user can generate presigned URLs for shop photos, not just shop owners or admins.
- Files: `CoffeePeek.MediaService/Controllers/PhotosController.cs:48`

---

## Test Coverage Gaps

**Shops Application Handlers — Zero Tests:**
- What's not tested: All handlers in `CoffeePeek.Shops.Application/Features/` — `SearchCoffeeShopsHandler`, `GetCoffeeShopHandler`, `CreateCheckInHandler`, `AddToFavoriteHandler`, `DeleteReviewFromCoffeeShopHandler`, `GetShopsInBoundsHandler`.
- Files: `CoffeePeek.Shops.Application/Features/` (entire directory)
- Risk: The ownership check gap in `DeleteReviewFromCoffeeShop` and the silent DomainException swallow in `CreateCheckInHandler` would both be caught by targeted unit tests.
- Priority: High

**Shops Domain — Only Equipment Tests:**
- What's not tested: `CoffeeShop` aggregate domain logic (`IsOpen`, `AddEquipment` duplicate prevention, `RotateRefreshToken`-style schedule edge cases), `Review.Create` validation, `CheckIn.Create` invariants.
- Files: `CoffeePeek.Shops.Domain.Tests/Entities/CoffeeShopAggregate/EquipmentTest.cs` (only 1 test file)
- Risk: Domain logic regressions go undetected.
- Priority: High

**Moderation Service — No Tests at All:**
- What's not tested: `CoffeePeek.Moderation.Application`, `CoffeePeek.Moderation.Infrastructure`, `CoffeePeek.Moderation.Domain`
- Files: No `*.Tests` project found for Moderation.
- Risk: Geocoding service failures, review status state machine transitions, and shop approval flows are untested.
- Priority: High

**Media Service — No Tests at All:**
- What's not tested: `GenerateAvatarUploadHandler`, `ConfirmPhotoHandler`, `MinIOStorageService`, presigned URL generation logic.
- Files: No `*.Tests` project found for MediaService.
- Risk: File size validation bypass, storage key generation collisions, and bucket routing errors go undetected.
- Priority: Medium

**Account Service — `UpdateEmail` Handler Has No Tests:**
- What's not tested: `CoffeePeek.Account.Application/Features/User/UpdateUserProfile/UpdateEmail/` — no corresponding test directory exists under `CoffeePeek.Account.Application.Tests/Features/User/UpdateUserProfile/`.
- Files: `CoffeePeek.Account.Application/Features/User/UpdateUserProfile/UpdateEmail/`
- Risk: Email change flow (re-confirmation requirement, uniqueness check) is untested.
- Priority: Medium

---

## Naming Inconsistencies (Accepted As-Is)

These typos exist in folder/project/namespace names and are accepted as-is per `CLAUDE.md`. They affect tooling discoverability and developer onboarding:

- `CoffePeek.AppHost`, `CoffePeek.ServiceDefaults` — missing one `e` (should be `CoffeePeek`)
- `CoffeePeek.Shops.Persistance` — missing `s` (should be `Persistence`); affects namespace, project file, and all `using` statements in the Shops bounded context
- `CoffeeShop.Moderation.Persistence` — wrong prefix (`CoffeeShop` instead of `CoffeePeek`)
- `CoffeePeek.Shared.Kernel/Extentions/` — directory and namespace typo (should be `Extensions`)

---

*Concerns audit: 2026-05-17*
