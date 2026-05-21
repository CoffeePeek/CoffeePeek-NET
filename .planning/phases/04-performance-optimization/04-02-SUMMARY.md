---
phase: 04-performance-optimization
plan: "02"
subsystem: CoffeePeek.Shops.Infrastructure
tags: [performance, ef-core, bulk-update, n-plus-one]
dependency_graph:
  requires: []
  provides: [bulk-username-update]
  affects: [CoffeePeek.Shops.Infrastructure]
tech_stack:
  added: []
  patterns: [EF Core ExecuteUpdateAsync bulk update]
key_files:
  modified:
    - CoffeePeek.Shops.Infrastructure/Consumers/UserNameChangedEventHandler.cs
    - CoffeePeek.Shops.Infrastructure/CoffeePeek.Shops.Infrastructure.csproj
decisions:
  - "ExecuteUpdateAsync bypasses EF Core change tracker — acceptable because UserName on Review is denormalized display field, not auditable"
  - "IReviewRepository and IUnitOfWork removed from constructor — ShopsDbContext injected directly for bulk operation"
  - "Added project reference CoffeePeek.Shops.Infrastructure -> CoffeePeek.Shops.Persistance (Infrastructure layer accessing DbContext directly for bulk ops)"
  - "Added Microsoft.EntityFrameworkCore.Relational package to Infrastructure project to resolve ExecuteUpdateAsync extension method"
metrics:
  duration: "~10 minutes"
  completed: "2026-05-20"
  tasks_total: 1
  tasks_completed: 1
  files_modified: 2
---

# Phase 04 Plan 02: UserNameChangedHandler N+1 Fix Summary

Replaced the N+1 UPDATE pattern in `UserNameChangedHandler` with a single `ExecuteUpdateAsync` call (PERF-05). The handler now issues one `UPDATE reviews SET user_name = $1 WHERE user_id = $2 AND user_name != $1` instead of loading all reviews into memory, iterating to call `UpdateUserName`, and then flushing the change tracker.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Replace GetByUserId loop with ExecuteUpdateAsync | 5437ce4 | CoffeePeek.Shops.Infrastructure/Consumers/UserNameChangedEventHandler.cs, CoffeePeek.Shops.Infrastructure/CoffeePeek.Shops.Infrastructure.csproj |

## Changes Made

### Before (N+1 UPDATEs)

```csharp
public class UserNameChangedHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
{
    public async Task Handle(UserNameChangedEvent message, CancellationToken cancellationToken = default)
    {
        var reviews = await reviewRepository.GetByUserId(message.UserId, cancellationToken);
        if (reviews.Length == 0) return;
        foreach (var review in reviews)
            review.UpdateUserName(message.NewUserName);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

### After (single UPDATE)

```csharp
public class UserNameChangedHandler(ShopsDbContext dbContext)
{
    public async Task Handle(UserNameChangedEvent message, CancellationToken cancellationToken = default)
    {
        // ExecuteUpdateAsync bypasses EF Core change tracker. Acceptable: UserName on Review is a denormalized display field.
        await dbContext.Reviews
            .Where(r => r.UserId == message.UserId && r.UserName != message.NewUserName)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.UserName, message.NewUserName), cancellationToken);
    }
}
```

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added project reference from Infrastructure to Persistance**
- **Found during:** Task 1 (build step)
- **Issue:** `ShopsDbContext` lives in `CoffeePeek.Shops.Persistance`; the Infrastructure project had no reference to it.
- **Fix:** Added `<ProjectReference Include="..\CoffeePeek.Shops.Persistance\CoffeePeek.Shops.Persistance.csproj" />` to the Infrastructure `.csproj`.
- **Files modified:** `CoffeePeek.Shops.Infrastructure/CoffeePeek.Shops.Infrastructure.csproj`
- **Commit:** 5437ce4

**2. [Rule 3 - Blocking] Added Microsoft.EntityFrameworkCore.Relational package to Infrastructure project**
- **Found during:** Task 1 (build step, `CS0234: EntityFrameworkCore namespace not found`)
- **Issue:** `ExecuteUpdateAsync` is an extension method in the EF Core Relational package; the compiler requires a direct package reference for the `using Microsoft.EntityFrameworkCore` directive to resolve.
- **Fix:** Added `<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" />` to the Infrastructure `.csproj`. The version is centrally managed in `Directory.Packages.props` — no version needed in the project file.
- **Files modified:** `CoffeePeek.Shops.Infrastructure/CoffeePeek.Shops.Infrastructure.csproj`
- **Commit:** 5437ce4

## Verification

- `dotnet build CoffeePeek.slnx` — 0 errors (170 pre-existing XML doc warnings, unrelated)
- `dotnet test CoffeePeek.Account.Application.Tests` — 141 passed, 0 failed
- `dotnet test CoffeePeek.Shops.Application.Tests` — 5 passed, 0 failed
- `grep "ExecuteUpdateAsync"` in handler — 2 matches (1 comment, 1 call)
- `grep "SaveChangesAsync|IReviewRepository|IUnitOfWork"` in handler — 0 matches
- `grep "ShopsDbContext"` in handler — 1 match

## Known Stubs

None.

## Threat Flags

None — no new network endpoints, auth paths, or schema changes. ExecuteUpdateAsync is parameterized by EF Core; no SQL injection risk.

## Self-Check: PASSED

- File exists: `/home/arseny/RiderProjects/CoffeePeek-NET/CoffeePeek.Shops.Infrastructure/Consumers/UserNameChangedEventHandler.cs` — FOUND
- Commit 5437ce4 — FOUND
