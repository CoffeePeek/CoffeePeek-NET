# Phase 2: Bug Fixes - Research

**Researched:** 2026-05-17
**Domain:** CoffeePeek Shops Service — cache keys, authorization, event handling, personalization
**Confidence:** HIGH

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| BUG-01 | Cache key mismatch for CoffeeBean — `ListPattern()` returns `"bean:list:*"` but write key is `"coffeebean:list:all"` | Exact strings confirmed in `CacheKey.cs` lines 129 and 134 |
| BUG-02 | `SearchCoffeeShopsHandler` returns empty `"Error"` string instead of descriptive message when Redis/factory returns null | Line 37 of `SearchCoffeeShopsHandler.cs` confirmed |
| BUG-03 | `DeleteReviewFromCoffeeShopHandler` does not check ownership before deleting — non-owner can delete any review | Handler confirmed, `Review.UserId` property confirmed, no `ForbiddenException` exists yet |
| BUG-04 | `CoffeeShopsController.GetCoffeeShop` doesn't pass `userId` to `GetCoffeeShopQuery` — authenticated users see wrong `IsFavorite`/`IsVisited` | Controller line 97 confirmed: `new GetCoffeeShopQuery(id)` ignores `userId` |
| BUG-05 | `CoffeeShopApprovedEventHandler` (via `CreateShopFromModerationService`) never calls `SaveChangesAsync()` after `shopRepository.Add()` | Confirmed in `CreateShopFromModerationService.cs` — no `IUnitOfWork` injected or called |
</phase_requirements>

---

## Summary

Phase 2 fixes five discrete functional bugs across the Shops Service. Four of the five bugs were confirmed by reading exact source files — no approximations. The fixes are surgical: one string constant, one error message, two controller-and-handler pairs, and one missing `SaveChangesAsync` call. No new packages are required. No schema migrations are required.

The most serious bug is **BUG-03** (any authenticated user can delete any review — a classic IDOR) and **BUG-05** (shop approved via moderation is silently dropped — data never reaches PostgreSQL). BUG-04 is a functional regression affecting every authenticated user viewing a coffee shop detail page.

**Primary recommendation:** Fix all five bugs in isolated commits, ordered by severity: BUG-05 (data loss), BUG-03 (security IDOR), BUG-04 (personalization broken), BUG-01 (cache never invalidated), BUG-02 (unhelpful error message).

---

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Cache key consistency (BUG-01) | Shared Domain (`CacheKey.cs`) | Application handler | String constant lives in `CoffeePeek.Shared.Domain`; read path in handler |
| Search error response (BUG-02) | Application handler | — | Error message constructed in `SearchCoffeeShopsHandler` |
| Review ownership check (BUG-03) | Application handler | Shared Kernel (new exception) | Business rule enforced at handler; HTTP mapping via existing `GlobalExceptionHandler` |
| UserId threading for personalization (BUG-04) | API Controller | Application handler (query) | Controller constructs `GetCoffeeShopQuery`; handler already handles `UserId` correctly |
| Shop event persistence (BUG-05) | Application Service | Infrastructure consumer | `CreateShopFromModerationService` builds and adds shop but never flushes; `ModerationShopApproveHandler` calls it |

---

## Bug Analysis (Exact File Locations)

### BUG-01: Cache Key Mismatch for CoffeeBean

**File:** `CoffeePeek.Shared.Domain/Interfaces/Infrastructure/CacheKey.cs`

**Write key** (line 129):
```csharp
public static CacheKey ListAll() => new(
    Key: "coffeebean:list:all",   // prefix = "coffeebean"
    ...);
```

**Invalidation pattern** (line 134):
```csharp
public static string ListPattern() => "bean:list:*";  // prefix = "bean" — MISMATCH
```

**Impact:** `CoffeeBean` cache entries written under `coffeebean:list:*` are never matched by the `bean:list:*` pattern. The `Categories.Shops.Patterns["dictionaries"]` (line 204) includes `CoffeeBean.ListPattern()` — so admin cache invalidation also silently does nothing for beans.

**Fix:** Change `ListPattern()` to return `"coffeebean:list:*"` so it matches the write key prefix. Single-line change in `CacheKey.cs`.

**Verification:** All other catalog types (`City`, `Equipment`, `Roaster`, `BrewMethod`) have matching prefixes in `ListAll()` and `ListPattern()` — confirmed by reading lines 106–157 of `CacheKey.cs`. [VERIFIED: codebase grep]

---

### BUG-02: SearchCoffeeShopsHandler Returns Empty "Error" String

**File:** `CoffeePeek.Shops.Application/Features/CoffeeShop/SearchCoffeeShops/SearchCoffeeShopsHandler.cs`, line 37:
```csharp
if (cachedResponse == null) return Response<GetCoffeeShopsResponse>.Error("Error");
```

**Context:** `redisService.GetAsync(cacheKey, factory, cancellationToken: ct)` calls the factory if Redis misses. The factory always returns a non-null `GetCoffeeShopsResponse`. The null-check is a defensive guard for unexpected future changes.

**Impact:** If this guard fires (factory returns null, or ICacheService implementation changes), the client receives `{ "isSuccess": false, "message": "Error" }` — no context about what failed.

**Fix:** Replace `"Error"` with a descriptive message: `"Failed to retrieve coffee shop search results"`. Single-line change.

---

### BUG-03: Missing Ownership Check in DeleteReviewFromCoffeeShopHandler

**Handler file:** `CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopHandler.cs`

Current handler (full, 27 lines):
```csharp
public async Task<Response> Handle(DeleteReviewFromCoffeeShopCommand request, 
    IReviewRepository reviewRepository,
    IUnitOfWork unitOfWork,
    CancellationToken cancellationToken)
{
    var review = await reviewRepository.GetById(request.ReviewId, cancellationToken);
    if (review == null)
        throw new NotFoundException($"{nameof(Review)} not found by id");

    review.SoftDelete();
    await unitOfWork.SaveChangesAsync(cancellationToken);
    return Response.Success();
}
```

No ownership check. `review.UserId` exists on the `Review` entity (line 12 of `Review.cs`).

**Command file:** `CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopCommand.cs`
```csharp
public record DeleteReviewFromCoffeeShopCommand(Guid ReviewId);
```
`RequestingUserId` is missing.

**Controller file:** `CoffeePeek.ShopsService/Controllers/CoffeeShopReviewsController.cs`, line 51:
```csharp
var command = new DeleteReviewFromCoffeeShopCommand(reviewId);
// userContext.GetUserIdOrThrow() is called at line 31 for CanCreateReview but NOT here for DeleteReview
```
The controller already injects `IUserContext userContext` — it's used for `CanCreateReview` but not wired into the delete command.

**Exception infrastructure:** `ForbiddenException` does NOT exist in `CoffeePeek.Shared.Kernel`. Available exceptions: `NotFoundException` (404), `UnauthorizedException` (401), `ValidationException` (400), `ConflictException` (409), `DatabaseException` (503). `GlobalExceptionHandler.GetStatusCode` uses `BaseException { StatusCode: not null }` as first case — a new `ForbiddenException` with `StatusCode = 403` will be handled correctly without any changes to the exception handler.

**Required changes:**
1. Create `CoffeePeek.Shared.Kernel/Exceptions/ForbiddenException.cs` (mirrors `UnauthorizedException` with `HttpStatusCode.Forbidden`)
2. Add `[property: JsonIgnore] Guid RequestingUserId` to `DeleteReviewFromCoffeeShopCommand` (follow the `RemoveFromFavoriteCommand` pattern)
3. In `DeleteReviewFromCoffeeShopHandler`: after fetching review, check `review.UserId != request.RequestingUserId` → throw `ForbiddenException("You do not have permission to delete this review")`
4. In `CoffeeShopReviewsController.DeleteReview`: set `command = command with { RequestingUserId = userContext.GetUserIdOrThrow() }`

**Pattern reference:** `RemoveFromFavoriteCommand([property: JsonIgnore] Guid UserId, Guid CoffeeShopId)` — exact same `[property: JsonIgnore]` pattern. [VERIFIED: codebase grep]

---

### BUG-04: GetCoffeeShop Doesn't Pass UserId

**Controller file:** `CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs`, line 97:
```csharp
var query = new GetCoffeeShopQuery(id);   // UserId omitted — defaults to null
```

**Query type:** `GetCoffeeShopQuery(Guid Id, Guid? UserId = null)` — `UserId` parameter exists, defaults to null.

**Handler:** `GetCoffeeShopHandler` at line 43 correctly conditionally checks `query.UserId.HasValue` and fetches `IsFavorite` / `IsVisited` only when `UserId` is present. The handler logic is correct. The bug is only in the controller.

**Comparison:** `GetCoffeeShops` (search endpoint) in the same controller at line 49 correctly passes `UserId: userContext.GetUserId()`.

**Fix:** Change line 97 from `new GetCoffeeShopQuery(id)` to `new GetCoffeeShopQuery(id, userContext.GetUserId())`. One token change. `GetUserId()` returns `Guid?` (null if not authenticated), which matches `Guid? UserId` on the query. [VERIFIED: codebase grep]

**Side effect:** The response for `GetCoffeeShop` is cached under `CacheKey.Shop.Detail(shopId)` with a 3-minute TTL. The cache stores a `CoffeeShopDetailsDto` WITHOUT `IsFavorite`/`IsVisited` (those are added after cache fetch, in the handler at lines 47-50). So passing `userId` to the query does NOT pollute the shared cache with user-specific data — the `IsFavorite`/`IsVisited` logic executes after cache retrieval. This is safe.

---

### BUG-05: CoffeeShopApprovedEventHandler Missing SaveChangesAsync

**Event flow:**
1. `ModerationShopApproveHandler.Handle(ModerationShopApprovedEvent)` (in `CoffeePeek.Shops.Infrastructure/Consumers/CoffeeShopApprovedEventHandler.cs`)
2. Delegates to `createShopService.CreateShopFromApprovedEventAsync(...)` (in `CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs`)
3. `CreateShopFromModerationService` calls `shopRepository.Add(shop)` (line 90) — adds to EF Core change tracker only
4. **No `SaveChangesAsync` call anywhere** — `CreateShopFromModerationService` does not inject `IUnitOfWork`

**Comparison:** `ModerationReviewApprovedHandler` (same Consumers folder) correctly calls `await unitOfWork.SaveChangesAsync(ct)` after `reviewRepository.Add(review)`. That's the correct pattern.

**IQueryCoffeeShopRepository.Add:** `void Add(CoffeeShop shop)` — synchronous, only adds to DbSet. Requires `SaveChangesAsync` to flush to PostgreSQL. [VERIFIED: codebase grep]

**Fix options:**

Option A — Fix in `ModerationShopApproveHandler` (Infrastructure consumer):
- Inject `IUnitOfWork` into `ModerationShopApproveHandler`
- Call `await unitOfWork.SaveChangesAsync()` after `createShopService.CreateShopFromApprovedEventAsync(...)`

Option B — Fix in `CreateShopFromModerationService` (Application service):
- Add `IUnitOfWork` to `CreateShopFromModerationService` constructor
- Call `await unitOfWork.SaveChangesAsync(cancellationToken)` at the end of `CreateShopFromApprovedEventAsync`

**Recommendation: Option B** — the service is responsible for the complete shop creation workflow. Callers should not need to know to flush. Also consistent with `ICreateShopFromModerationService` contract which returns a `Guid` (the shop id) — the caller expects a completed creation, not a pending one. Option A would mean the infrastructure consumer knows about persistence details.

`IUnitOfWork` is already registered in the Shops persistence DI and available throughout the Shops service.

---

## Standard Stack

No new packages required. All fixes use existing framework and project infrastructure.

| Component | Version | Notes |
|-----------|---------|-------|
| .NET / C# | 10 / 14 | No change |
| `CoffeePeek.Shared.Kernel` | project | Add `ForbiddenException.cs` |
| `CoffeePeek.Shared.Domain` | project | Fix `CacheKey.CoffeeBean.ListPattern()` |
| `CoffeePeek.Shops.Application` | project | Fix handler + command for BUG-02, BUG-03 |
| `CoffeePeek.ShopsService` | project | Fix controller for BUG-03, BUG-04 |

---

## Package Legitimacy Audit

No external packages are installed in this phase. Section is not applicable.

---

## Architecture Patterns

### Pattern 1: [property: JsonIgnore] for Server-Side User IDs in Commands

User-supplied values that come from the auth context (not the request body) are marked `[property: JsonIgnore]` on command records so Wolverine/JSON serializers cannot bind them from the wire.

```csharp
// Source: CoffeePeek.Shops.Application/Features/Favorite/RemoveFromFavorite/RemoveFromFavoriteCommand.cs
public record RemoveFromFavoriteCommand([property: JsonIgnore] Guid UserId, Guid CoffeeShopId);
```

The controller fills them in before invoking:
```csharp
var command = new RemoveFromFavoriteCommand(userContext.GetUserIdOrThrow(), id);
// or using 'with' expression for existing command objects:
command = command with { UserId = userContext.GetUserIdOrThrow() };
```

### Pattern 2: BaseException Subclass for HTTP Status Codes

All domain-level HTTP errors extend `BaseException` and pass `(int)HttpStatusCode.X` as `statusCode`:

```csharp
// Pattern from CoffeePeek.Shared.Kernel/Exceptions/UnauthorizedException.cs
public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Unauthorized access", string? errorCode = null)
        : base(message, errorCode, (int)HttpStatusCode.Unauthorized) { }
}
```

`GlobalExceptionHandler.GetStatusCode` handles all `BaseException` subclasses with `{ StatusCode: not null }` — new `ForbiddenException` requires zero changes to the handler.

### Pattern 3: IUnitOfWork After Repository.Add

```csharp
// Pattern from CoffeePeek.Shops.Infrastructure/Consumers/ModerationReviewApprovedHandler.cs
reviewRepository.Add(review);
await unitOfWork.SaveChangesAsync(ct);
```

`Repository.Add()` only tracks the entity; `SaveChangesAsync` flushes to PostgreSQL. Every repository.Add must be paired with a SaveChangesAsync.

### Pattern 4: userContext.GetUserId() for Optional Authentication

`GetUserId()` returns `Guid?` (null if not authenticated). Used for endpoints accessible to both anonymous and authenticated users:

```csharp
// Pattern from CoffeeShopsController.GetCoffeeShops (line 49)
UserId: userContext.GetUserId(),
```

`GetUserIdOrThrow()` is used for endpoints requiring authentication. `GetCoffeeShop` is not decorated with `[Authorize]`, so `GetUserId()` is the correct choice.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| HTTP 403 response | Custom return value from handler | `ForbiddenException : BaseException` | `GlobalExceptionHandler` already maps `BaseException.StatusCode` → HTTP status; consistent with all other error paths in the codebase |
| Cache pattern invalidation | Custom key scanning | Existing `CacheKey.CoffeeBean.ListPattern()` after fix | The pattern string is already wired into `Categories.Shops.Patterns` |

---

## Common Pitfalls

### Pitfall 1: Cache Invalidation Pattern Prefix Must Match Write Key Prefix

**What goes wrong:** `ListAll()` returns key `"coffeebean:list:all"` but `ListPattern()` returns `"bean:list:*"`. Pattern-based invalidation via `KEYS "bean:list:*"` finds nothing.
**Why it happens:** Pattern and key were written independently (possibly renamed one without the other).
**How to avoid:** When writing `ListPattern()`, always derive from the same string literal used in `ListAll()`.Key: `"{prefix}:list:all"` → Pattern: `"{prefix}:list:*"`.
**Warning signs:** After adding a bean, the catalog endpoint still returns stale data after TTL expiry.

### Pitfall 2: Forgetting [property: JsonIgnore] on Server-Populated Fields

**What goes wrong:** `RequestingUserId` on a command record is serializable — Wolverine's message bus could theoretically receive a spoofed value from a crafted message if the bus is multi-tenant.
**Why it happens:** `[JsonIgnore]` must be on the `property:` target (not the parameter) for record types.
**How to avoid:** Always use `[property: JsonIgnore]` for fields populated from `IUserContext` or request headers.

### Pitfall 3: IForbiddenException Must Not Be `UnauthorizedException` (401 vs 403)

**What goes wrong:** Using `UnauthorizedException` for "wrong owner" returns HTTP 401, not 403. HTTP 401 means "not authenticated"; HTTP 403 means "authenticated but not allowed."
**Why it happens:** Developers conflate the two.
**How to avoid:** Create a distinct `ForbiddenException` with `HttpStatusCode.Forbidden`. Do not reuse `UnauthorizedException`.

### Pitfall 4: SaveChangesAsync Must Be Called Within the Same DbContext Scope

**What goes wrong:** `CreateShopFromModerationService` runs within the Wolverine message handler's DI scope. If `IUnitOfWork` is injected but `SaveChangesAsync` is deferred to a different scope, it may operate on a different `DbContext` instance.
**Why it happens:** Scoped lifetime mismatch.
**How to avoid:** Inject `IUnitOfWork` directly into `CreateShopFromModerationService` constructor (like `ModerationReviewApprovedHandler` does). Since the service is already `Scoped` (all application services are), the same `ShopsDbContext` instance will be shared.

### Pitfall 5: GetCoffeeShop Response is Cached Without UserId — Don't Change That

**What goes wrong:** If the fix naively bakes `IsFavorite`/`IsVisited` into the cached DTO, all users share the same cached value.
**Why it happens:** Confusion about what is cached vs. what is post-processed.
**How to avoid:** The existing `GetCoffeeShopHandler` correctly fetches `IsFavorite`/`IsVisited` AFTER cache retrieval (lines 43-50). The fix is only in the controller — pass `userId` to the query, and the handler already does the right thing.

---

## Code Examples

### New ForbiddenException (BUG-03)

```csharp
// File: CoffeePeek.Shared.Kernel/Exceptions/ForbiddenException.cs
using System.Net;

namespace CoffeePeek.Shared.Kernel.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Access forbidden", string? errorCode = null)
        : base(message, errorCode, (int)HttpStatusCode.Forbidden)
    {
    }

    public ForbiddenException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, (int)HttpStatusCode.Forbidden)
    {
    }
}
```

### Updated DeleteReviewFromCoffeeShopCommand (BUG-03)

```csharp
// File: CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopCommand.cs
namespace CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;

public record DeleteReviewFromCoffeeShopCommand(Guid ReviewId, [property: JsonIgnore] Guid RequestingUserId);
```

### Updated DeleteReviewFromCoffeeShopHandler (BUG-03)

```csharp
var review = await reviewRepository.GetById(request.ReviewId, cancellationToken);
if (review == null)
    throw new NotFoundException($"{nameof(Review)} not found by id");

if (review.UserId != request.RequestingUserId)
    throw new ForbiddenException("You do not have permission to delete this review");

review.SoftDelete();
await unitOfWork.SaveChangesAsync(cancellationToken);
return Response.Success();
```

### Updated CoffeeShopsController.GetCoffeeShop (BUG-04)

```csharp
var query = new GetCoffeeShopQuery(id, userContext.GetUserId());
```

### Updated CreateShopFromModerationService (BUG-05)

Constructor addition:
```csharp
public class CreateShopFromModerationService(
    IQueryCoffeeShopRepository shopRepository,
    IQueryCoffeeBeanRepository coffeeBeanRepository,
    IQueryEquipmentRepository equipmentRepository,
    IQueryRoasterRepository roasterRepository,
    IQueryBrewMethodRepository brewMethodRepository,
    IUnitOfWork unitOfWork,                        // ADD
    ILogger<CreateShopFromModerationService> logger) : ICreateShopFromModerationService
```

End of `CreateShopFromApprovedEventAsync`, after `shopRepository.Add(shop)`:
```csharp
shopRepository.Add(shop);
await unitOfWork.SaveChangesAsync(cancellationToken);   // ADD

logger.LogInformation("Shop {ShopId} successfully created from moderation event {ModerationId}", ...);
return shop.Id;
```

---

## Validation Architecture

`workflow.nyquist_validation: true` — this section is required.

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit v3 3.2.2 |
| Config file | none (xunit auto-discovered from csproj) |
| Quick run command | `dotnet test CoffeePeek.slnx --filter "FullyQualifiedName~BugFix"` |
| Full suite command | `dotnet test CoffeePeek.slnx` |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Notes |
|--------|----------|-----------|-------|
| BUG-01 | `CoffeeBean.ListPattern()` matches `CoffeeBean.ListAll().Key` prefix | unit | Pure string assertion — no mocks needed |
| BUG-02 | `SearchCoffeeShopsHandler` returns descriptive message on null response | unit | Mock `ICacheService.GetAsync` to return null |
| BUG-03 | Delete review by non-owner throws `ForbiddenException` | unit | Mock `IReviewRepository.GetById` with UserId mismatch |
| BUG-03 | Delete review by owner succeeds | unit | Happy path regression |
| BUG-04 | `GetCoffeeShopQuery` carries `UserId` from authenticated context | unit/controller | Verify query construction passes userId |
| BUG-05 | `CreateShopFromModerationService` calls `SaveChangesAsync` after `Add` | unit | Mock `IUnitOfWork`, verify `SaveChangesAsync` called once |

### Existing Test Projects

| Project | Covers | Status |
|---------|--------|--------|
| `CoffeePeek.Account.Application.Tests` | Account Application handlers | Exists, has tests |
| `CoffeePeek.Account.Domain.Tests` | Account domain entities | Exists |
| `CoffeePeek.Shops.Domain.Tests` | Shops domain entities | Exists (one test file: `EquipmentTest.cs`) |

**No `CoffeePeek.Shops.Application.Tests` project exists.** Phase 5 (TEST-01 through TEST-05) creates this project. For Phase 2, tests for BUG-03 / BUG-05 should be added to a new `CoffeePeek.Shops.Application.Tests` project or (simpler) as inline unit tests in the same test project that Phase 5 will expand. Creating a minimal project in Phase 2 aligns with Phase 5's requirements and does not conflict with deferred scope.

**Alternative:** Add BUG-01 string test to `CoffeePeek.Shops.Domain.Tests` (it tests a domain shared type). Add BUG-03/BUG-05 tests to a new `CoffeePeek.Shops.Application.Tests` project created in Phase 2 (Phase 5 expands it).

### Wave 0 Gaps

- [ ] `CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj` — new project (mirrors structure of `CoffeePeek.Account.Application.Tests.csproj`)
- [ ] `CoffeePeek.Shops.Application.Tests/GlobalUsings.cs` — global using imports
- [ ] `CoffeePeek.Shops.Application.Tests/Features/Review/DeleteReviewFromCoffeeShopHandlerTests.cs`
- [ ] `CoffeePeek.Shops.Application.Tests/Services/CreateShopFromModerationServiceTests.cs`

---

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V4 Access Control | YES (BUG-03) | `ForbiddenException` thrown when `review.UserId != requestingUserId`; HTTP 403 |
| V5 Input Validation | no | |
| V2 Authentication | no | |

### Known Threat Patterns

| Pattern | STRIDE | Mitigation Applied by BUG-03 Fix |
|---------|--------|-----------------------------------|
| IDOR (Insecure Direct Object Reference) | Tampering | Verify resource ownership before mutation |

---

## Open Questions (RESOLVED)

1. **Should BUG-05 be fixed in `CreateShopFromModerationService` or in `ModerationShopApproveHandler`?**
   - What we know: both options work; Option B (service) is cleaner
   - What's unclear: whether `CreateShopFromModerationService` is called from any other place that already provides a surrounding `SaveChanges`
   - RESOLVED: Grepped `CreateShopFromApprovedEventAsync` — called only from `ModerationShopApproveHandler`. Fix is in the service (Option B), as implemented by Plan 02-04.

2. **Should Phase 2 create `CoffeePeek.Shops.Application.Tests` or defer to Phase 5?**
   - What we know: Phase 5 (TEST-03) explicitly requires a BUG-03 regression test
   - RESOLVED: Project is created in Phase 2 (Plan 02-02) with minimal BUG-03 and BUG-05 tests; Phase 5 expands it. Plans 02-03 and 02-04 add their regression tests to this project.

---

## Environment Availability

Step 2.6: SKIPPED — this phase contains only code edits and string constant fixes. No external CLI tools, databases, or services are invoked during the fix itself. The Shops Service runtime (PostgreSQL, Redis, RabbitMQ) is required to observe/validate the bugs but not to apply the code changes.

---

## Sources

### Primary (HIGH confidence)

All findings were verified by direct file reads of production source files. No training-data assumptions were made for bug locations.

| File | Finding |
|------|---------|
| `CoffeePeek.Shared.Domain/Interfaces/Infrastructure/CacheKey.cs` | BUG-01: confirmed `"coffeebean:list:all"` vs `"bean:list:*"` mismatch |
| `CoffeePeek.Shops.Application/Features/CoffeeShop/SearchCoffeeShops/SearchCoffeeShopsHandler.cs` | BUG-02: line 37 `Response.Error("Error")` |
| `CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopHandler.cs` | BUG-03: no ownership check |
| `CoffeePeek.Shops.Domain/Aggregates/ReviewAggregate/Review.cs` | BUG-03: `UserId` property exists on `Review` |
| `CoffeePeek.Shared.Kernel/Exceptions/` (all files) | BUG-03: `ForbiddenException` does not exist; `GlobalExceptionHandler` pattern confirmed |
| `CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs` | BUG-04: line 97 `new GetCoffeeShopQuery(id)` — no `userId` argument |
| `CoffeePeek.Shops.Application/Features/CoffeeShop/GetCoffeeShop/GetCoffeeShopHandler.cs` | BUG-04: handler already handles `UserId` correctly at lines 43-50 |
| `CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs` | BUG-05: `shopRepository.Add(shop)` at line 90, no `IUnitOfWork`, no `SaveChangesAsync` |
| `CoffeePeek.Shops.Infrastructure/Consumers/ModerationReviewApprovedHandler.cs` | BUG-05: correct reference pattern with `SaveChangesAsync` |

---

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| — | — | — | — |

**All claims in this research were verified by direct file reads.** No assumed knowledge was used for bug locations, string values, type signatures, or fix strategies.

---

## Metadata

**Confidence breakdown:**
- Bug locations and exact strings: HIGH — confirmed by file reads
- Fix strategies: HIGH — confirmed by reference patterns in same codebase
- Test project creation approach: MEDIUM — Phase 5 scope boundary judgment call

**Research date:** 2026-05-17
**Valid until:** 2026-06-16 (30 days; codebase is stable)
