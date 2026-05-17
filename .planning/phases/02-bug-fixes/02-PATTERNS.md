# Phase 2: Bug Fixes - Pattern Map

**Mapped:** 2026-05-17
**Files analyzed:** 11
**Analogs found:** 11 / 11

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `CoffeePeek.Shared.Domain/Interfaces/Infrastructure/CacheKey.cs` | utility | transform | Self (existing file, same class) | exact |
| `CoffeePeek.Shops.Application/Features/CoffeeShop/SearchCoffeeShops/SearchCoffeeShopsHandler.cs` | handler | request-response | Self (existing file, same class) | exact |
| `CoffeePeek.Shared.Kernel/Exceptions/ForbiddenException.cs` | utility | — | `CoffeePeek.Shared.Kernel/Exceptions/UnauthorizedException.cs` | exact |
| `CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopCommand.cs` | command | request-response | `CoffeePeek.Shops.Application/Features/Favorite/RemoveFromFavorite/RemoveFromFavoriteCommand.cs` | exact |
| `CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopHandler.cs` | handler | request-response | Self (existing file) + `ModerationReviewApprovedHandler.cs` | exact |
| `CoffeePeek.ShopsService/Controllers/CoffeeShopReviewsController.cs` | controller | request-response | Self (existing file, same controller) | exact |
| `CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs` | controller | request-response | Self (existing file, same controller) | exact |
| `CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs` | service | event-driven | `CoffeePeek.Shops.Infrastructure/Consumers/ModerationReviewApprovedHandler.cs` | role-match |
| `CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj` | config | — | `CoffeePeek.Account.Application.Tests/CoffeePeek.Account.Application.Tests.csproj` | exact |
| `CoffeePeek.Shops.Application.Tests/Features/Review/DeleteReviewFromCoffeeShopHandlerTests.cs` | test | request-response | `CoffeePeek.Account.Application.Tests/Features/User/DeleteUser/DeleteUserHandlerTests.cs` | exact |
| `CoffeePeek.Shops.Application.Tests/Services/CreateShopFromModerationServiceTests.cs` | test | event-driven | `CoffeePeek.Account.Application.Tests/Features/Auth/Login/LoginUserHandlerTests.cs` | role-match |

---

## Pattern Assignments

### `CoffeePeek.Shared.Domain/Interfaces/Infrastructure/CacheKey.cs` (utility, string constant fix)

**Analog:** Self — the file already exists and follows its own internal convention.

**The bug** (lines 128-135 of `CacheKey.cs`):
```csharp
// CoffeeBean static class — write key uses prefix "coffeebean", pattern uses "bean" — MISMATCH
public static CacheKey ListAll() => new(
    Key: "coffeebean:list:all",   // line 129
    DefaultTtl: TimeSpan.FromDays(1),
    Description: "All coffee beans list",
    Service: "ShopsService");

public static string ListPattern() => "bean:list:*";  // line 134 — BUG: should be "coffeebean:list:*"
```

**Correct pattern** — every other catalog type in the same file follows `{prefix}:list:all` → `{prefix}:list:*`:
```csharp
// City (lines 106-113) — reference pattern
public static CacheKey ListAll() => new(Key: "city:list:all", ...);
public static string ListPattern() => "city:list:*";

// Equipment (lines 116-124)
public static CacheKey ListAll() => new(Key: "equipment:list:all", ...);
public static string ListPattern() => "equipment:list:*";
```

**Fix:** Change line 134 from `"bean:list:*"` to `"coffeebean:list:*"`.

---

### `CoffeePeek.Shops.Application/Features/CoffeeShop/SearchCoffeeShops/SearchCoffeeShopsHandler.cs` (handler, request-response)

**Analog:** Self — the file already exists.

**The bug** (line 37):
```csharp
if (cachedResponse == null) return Response<GetCoffeeShopsResponse>.Error("Error");
```

**Fix:** Replace `"Error"` with a descriptive message:
```csharp
if (cachedResponse == null) return Response<GetCoffeeShopsResponse>.Error("Failed to retrieve coffee shop search results");
```

**Response pattern** reference (same file, line 52 — correct usage):
```csharp
return Response<GetCoffeeShopsResponse>.Success(cachedResponse);
```

---

### `CoffeePeek.Shared.Kernel/Exceptions/ForbiddenException.cs` (utility, new file)

**Analog:** `CoffeePeek.Shared.Kernel/Exceptions/UnauthorizedException.cs` (lines 1-16)

**Copy this file exactly**, swapping `Unauthorized` → `Forbidden` and `HttpStatusCode.Unauthorized` → `HttpStatusCode.Forbidden`:

```csharp
using System.Net;

namespace CoffeePeek.Shared.Kernel.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Unauthorized access", string? errorCode = null)
        : base(message, errorCode, (int)HttpStatusCode.Unauthorized)
    {
    }

    public UnauthorizedException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, (int)HttpStatusCode.Unauthorized)
    {
    }
}
```

**Resulting new file:**
```csharp
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

**Why no changes to GlobalExceptionHandler:** `BaseException { StatusCode: not null }` is the first match arm — any new `BaseException` subclass with a `StatusCode` is handled automatically. `ForbiddenException` passes `(int)HttpStatusCode.Forbidden` = 403 to `StatusCode`.

---

### `CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopCommand.cs` (command, request-response)

**Analog:** `CoffeePeek.Shops.Application/Features/Favorite/RemoveFromFavorite/RemoveFromFavoriteCommand.cs` (lines 1-5)

**Analog file (full):**
```csharp
using System.Text.Json.Serialization;

namespace CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;

public record RemoveFromFavoriteCommand([property: JsonIgnore] Guid UserId, Guid CoffeeShopId);
```

**Current command to modify** (lines 1-3):
```csharp
namespace CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;

public record DeleteReviewFromCoffeeShopCommand(Guid ReviewId);
```

**After fix** — add `using` and `[property: JsonIgnore] Guid RequestingUserId`:
```csharp
using System.Text.Json.Serialization;

namespace CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;

public record DeleteReviewFromCoffeeShopCommand(Guid ReviewId, [property: JsonIgnore] Guid RequestingUserId);
```

**Key rule:** `[property: JsonIgnore]` (not `[JsonIgnore]`) is required for record positional parameters so the attribute targets the auto-generated property, preventing Wolverine/JSON binding from accepting it from the wire.

---

### `CoffeePeek.Shops.Application/Features/Review/DeleteReviewFromCoffeeShop/DeleteReviewFromCoffeeShopHandler.cs` (handler, request-response)

**Analog:** Self (existing file) + ownership check from RESEARCH.md

**Current handler body** (lines 10-27 — full file):
```csharp
public class DeleteReviewFromCoffeeShopHandler
{
    public async Task<Response> Handle(DeleteReviewFromCoffeeShopCommand request,
        IReviewRepository reviewRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetById(request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException($"{nameof(Review)} not found by id");
        }

        review.SoftDelete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success();
    }
}
```

**After fix** — insert ownership check after null check:
```csharp
if (review.UserId != request.RequestingUserId)
    throw new ForbiddenException("You do not have permission to delete this review");
```

**Imports to add** — `ForbiddenException` is in the same `CoffeePeek.Shared.Kernel.Exceptions` namespace already imported. Confirm the existing import block covers it (line 2: `using CoffeePeek.Shared.Kernel.Exceptions;`).

**Review.UserId** — confirmed at `CoffeePeek.Shops.Domain/Aggregates/ReviewAggregate/Review.cs` line 12: `public Guid UserId { get; private set; }`.

---

### `CoffeePeek.ShopsService/Controllers/CoffeeShopReviewsController.cs` (controller, request-response)

**Analog:** Self — existing controller. `GetUserId()` vs `GetUserIdOrThrow()` pattern from same file.

**Current delete action** (lines 49-55):
```csharp
public async Task<IActionResult> DeleteReview(Guid shopId, Guid reviewId)
{
    var command = new DeleteReviewFromCoffeeShopCommand(reviewId);
    var response = await bus.InvokeAsync<Response>(command);

    return response.IsSuccess ? NoContent() : NotFound(response);
}
```

**Reference pattern** in same file (lines 31-33) — `GetUserIdOrThrow()` for `[Authorize]` endpoints:
```csharp
var userId = userContext.GetUserIdOrThrow();
var query = new CanCreateCoffeeShopReviewQuery(userId, shopId);
```

**After fix** — wire `RequestingUserId` via `with` expression (preferred for existing command objects):
```csharp
public async Task<IActionResult> DeleteReview(Guid shopId, Guid reviewId)
{
    var command = new DeleteReviewFromCoffeeShopCommand(reviewId, userContext.GetUserIdOrThrow());
    var response = await bus.InvokeAsync<Response>(command);

    return response.IsSuccess ? NoContent() : NotFound(response);
}
```

**Why `GetUserIdOrThrow()`:** `CoffeeShopReviewsController` is decorated `[Authorize]` (line 12), so unauthenticated requests never reach the action. `GetUserIdOrThrow()` is correct; `GetUserId()` (nullable) would be wrong here.

---

### `CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs` (controller, request-response)

**Analog:** Self — the same controller's `GetCoffeeShops` action already does the correct pattern.

**Correct pattern** (lines 48-49 — `GetCoffeeShops`):
```csharp
var query = new SearchCoffeeShopsQuery(
    UserId: userContext.GetUserId(),   // Guid? — null for anonymous
    ...
```

**Buggy line** (line 97 — `GetCoffeeShop`):
```csharp
var query = new GetCoffeeShopQuery(id);   // UserId omitted — always null
```

**After fix:**
```csharp
var query = new GetCoffeeShopQuery(id, userContext.GetUserId());
```

**Why `GetUserId()` (nullable):** `GetCoffeeShop` is NOT decorated with `[Authorize]`. `GetUserId()` returns `Guid?` which matches `Guid? UserId = null` on `GetCoffeeShopQuery`. Anonymous users get `null`, authenticated users get their ID — handler already handles both cases correctly.

**Cache safety:** Handler fetches `IsFavorite`/`IsVisited` AFTER cache retrieval (post-process). The cached DTO does not include user-specific data. Passing `userId` to the query is safe.

---

### `CoffeePeek.Shops.Application/Services/CreateShopFromModerationService.cs` (service, event-driven)

**Analog:** `CoffeePeek.Shops.Infrastructure/Consumers/ModerationReviewApprovedHandler.cs` — correct reference pattern with `IUnitOfWork` + `SaveChangesAsync`.

**Reference pattern** (lines 1-39 of `ModerationReviewApprovedHandler.cs`):
```csharp
public static class ModerationReviewApprovedHandler
{
    public static async Task<ReviewAddedEvent?> Handle(
        ModerationReviewApprovedEvent @event,
        IReviewRepository reviewRepository,
        IUnitOfWork unitOfWork,             // injected
        CancellationToken ct)
    {
        // ...
        reviewRepository.Add(review);
        await unitOfWork.SaveChangesAsync(ct);   // called after Add
        return new ReviewAddedEvent(...);
    }
}
```

**Current service constructor** (lines 8-14 of `CreateShopFromModerationService.cs`) — missing `IUnitOfWork`:
```csharp
public class CreateShopFromModerationService(
    IQueryCoffeeShopRepository shopRepository,
    IQueryCoffeeBeanRepository coffeeBeanRepository,
    IQueryEquipmentRepository equipmentRepository,
    IQueryRoasterRepository roasterRepository,
    IQueryBrewMethodRepository brewMethodRepository,
    ILogger<CreateShopFromModerationService> logger) : ICreateShopFromModerationService
```

**After fix** — add `IUnitOfWork unitOfWork` before the logger parameter:
```csharp
public class CreateShopFromModerationService(
    IQueryCoffeeShopRepository shopRepository,
    IQueryCoffeeBeanRepository coffeeBeanRepository,
    IQueryEquipmentRepository equipmentRepository,
    IQueryRoasterRepository roasterRepository,
    IQueryBrewMethodRepository brewMethodRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateShopFromModerationService> logger) : ICreateShopFromModerationService
```

**Current end of method** (lines 90-96 — after `shopRepository.Add(shop)`):
```csharp
        shopRepository.Add(shop);

        logger.LogInformation("Shop {ShopId} successfully created from moderation event {ModerationId}", shop.Id,
            moderationId);

        return shop.Id;
```

**After fix** — insert `SaveChangesAsync` after `Add`, before logging:
```csharp
        shopRepository.Add(shop);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Shop {ShopId} successfully created from moderation event {ModerationId}", shop.Id,
            moderationId);

        return shop.Id;
```

**`IUnitOfWork` availability:** Already registered in Shops persistence DI. `CreateShopFromModerationService` is `Scoped`, so it shares the same `ShopsDbContext` instance as the repository — correct scope.

---

### `CoffeePeek.Shops.Application.Tests/CoffeePeek.Shops.Application.Tests.csproj` (config, new project)

**Analog:** `CoffeePeek.Account.Application.Tests/CoffeePeek.Account.Application.Tests.csproj` (full file, 22 lines)

**Copy pattern:**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FluentAssertions"  />
    <PackageReference Include="JetBrains.Annotations"/>
    <PackageReference Include="Microsoft.NET.Test.Sdk"  />
    <PackageReference Include="Moq" />
    <PackageReference Include="xunit.v3"  />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\CoffeePeek.Shops.Application\CoffeePeek.Shops.Application.csproj" />
  </ItemGroup>

</Project>
```

**Differences from analog:**
- `ProjectReference` points to `CoffeePeek.Shops.Application` (not `Account.Application`)
- No `coverlet.collector` in Account.Application.Tests — add it to align with `CoffeePeek.Shops.Domain.Tests.csproj` pattern which does include it
- No `<ImplicitUsings>enable</ImplicitUsings>` in Account.Application.Tests — add to match Domain.Tests

**Note:** `<OutputType>Exe</OutputType>` is required for xUnit v3 auto-generated entry point (required by xunit.v3). Keep it.

---

### `CoffeePeek.Shops.Application.Tests/Features/Review/DeleteReviewFromCoffeeShopHandlerTests.cs` (test, request-response)

**Analog:** `CoffeePeek.Account.Application.Tests/Features/User/DeleteUser/DeleteUserHandlerTests.cs` (full file, 71 lines)

**Test class structure pattern** (lines 14-19):
```csharp
public class DeleteUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;
```

**Factory helper pattern** (lines 21-24):
```csharp
private static DomainUser CreateUser()
{
    var role = Role.Create("User");
    return DomainUser.Register("user@example.com", "testuser", "hash", role);
}
```

**Exception assertion pattern** from `LoginUserHandlerTests.cs` (lines 62-70):
```csharp
Func<Task> act = () => LoginUserHandler.Handle(command, _authServiceMock.Object, _filter, _ct);
await act.Should().ThrowAsync<NotFoundException>();
```

**SaveChanges verification pattern** (lines 49-52 of DeleteUserHandlerTests):
```csharp
_unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
```

**Apply to `DeleteReviewFromCoffeeShopHandlerTests`:**
- Mock `IReviewRepository` + `IUnitOfWork`
- Helper that creates a `Review` entity via `Review.Create(...)` (factory method on domain entity)
- Test 1: reviewer owns the review → `SoftDelete()` called, `SaveChangesAsync` called once, returns `Response.IsSuccess == true`
- Test 2: non-owner (`review.UserId != requestingUserId`) → `ThrowAsync<ForbiddenException>()`
- Test 3: review not found → `ThrowAsync<NotFoundException>()`

**Namespace:** `CoffeePeek.Shops.Application.Tests.Features.Review.DeleteReviewFromCoffeeShop`

---

### `CoffeePeek.Shops.Application.Tests/Services/CreateShopFromModerationServiceTests.cs` (test, event-driven)

**Analog:** `CoffeePeek.Account.Application.Tests/Features/Auth/Login/LoginUserHandlerTests.cs` — service mock injection pattern.

**Mock injection pattern** (lines 17-19):
```csharp
private readonly Mock<IAuthService> _authServiceMock = new();
private readonly EmailExistenceFilter _filter = new(1000, 0.01);
private readonly CancellationToken _ct = CancellationToken.None;
```

**Verify-once pattern** from `DeleteUserHandlerTests.cs` (line 51):
```csharp
_unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
```

**Apply to `CreateShopFromModerationServiceTests`:**
- Mock all constructor dependencies: `IQueryCoffeeShopRepository`, `IQueryCoffeeBeanRepository`, `IQueryEquipmentRepository`, `IQueryRoasterRepository`, `IQueryBrewMethodRepository`, `IUnitOfWork`, `ILogger<CreateShopFromModerationService>`
- Create `ShopDto` test fixture (minimal valid data)
- Test 1 (BUG-05 regression): `CreateShopFromApprovedEventAsync` calls `unitOfWork.SaveChangesAsync` exactly once after `shopRepository.Add`
- Test 2: returns `Guid` (the shop id) on success
- Test 3: when `shopRepository.ExistsByModerationId` returns `true`, throws `InvalidOperationException` (existing guard, do not regress)

**Namespace:** `CoffeePeek.Shops.Application.Tests.Services`

---

## Shared Patterns

### Exception Subclass Pattern
**Source:** `CoffeePeek.Shared.Kernel/Exceptions/UnauthorizedException.cs` (full file, 16 lines)
**Apply to:** `ForbiddenException.cs`
```csharp
// Template — swap class name and HttpStatusCode value
public class {Name}Exception : BaseException
{
    public {Name}Exception(string message = "{Default message}", string? errorCode = null)
        : base(message, errorCode, (int)HttpStatusCode.{Code}) { }

    public {Name}Exception(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException, errorCode, (int)HttpStatusCode.{Code}) { }
}
```

### [property: JsonIgnore] for Server-Side Fields in Commands
**Source:** `CoffeePeek.Shops.Application/Features/Favorite/RemoveFromFavorite/RemoveFromFavoriteCommand.cs` (line 5)
**Apply to:** `DeleteReviewFromCoffeeShopCommand.cs`
```csharp
using System.Text.Json.Serialization;
// ...
public record MyCommand(Guid EntityId, [property: JsonIgnore] Guid RequestingUserId);
```

### IUnitOfWork.SaveChangesAsync After Repository.Add
**Source:** `CoffeePeek.Shops.Infrastructure/Consumers/ModerationReviewApprovedHandler.cs` (lines 33-35)
**Apply to:** `CreateShopFromModerationService.cs`
```csharp
reviewRepository.Add(review);          // or shopRepository.Add(shop)
await unitOfWork.SaveChangesAsync(ct); // always paired — never omit
```

### userContext.GetUserId() vs GetUserIdOrThrow()
**Source:** `CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs` (line 49) and `CoffeeShopReviewsController.cs` (line 31)
**Apply to:** Controller fixes in BUG-03 and BUG-04
```csharp
// [Authorize] endpoint — throws 401 if not authenticated
userContext.GetUserIdOrThrow()   // → Guid

// Unauthenticated-friendly endpoint — null for anonymous
userContext.GetUserId()          // → Guid?
```

### Handler Test Class Structure
**Source:** `CoffeePeek.Account.Application.Tests/Features/User/DeleteUser/DeleteUserHandlerTests.cs` (lines 14-52)
**Apply to:** All new test classes in `CoffeePeek.Shops.Application.Tests`
```csharp
public class {Handler}Tests
{
    private readonly Mock<IDependency> _depMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    private static {Entity} Create{Entity}() => /* factory method */;

    [Fact]
    public async Task Handle_{Scenario}_{ExpectedOutcome}()
    {
        // Arrange / Act / Assert
    }
}
```

---

## No Analog Found

All files have close analogs in the codebase. No entries in this section.

---

## Metadata

**Analog search scope:** All .cs files under the solution root
**Files scanned:** 14 source files read directly
**Pattern extraction date:** 2026-05-17
