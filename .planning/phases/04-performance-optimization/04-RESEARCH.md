# Phase 4: Performance Optimization - Research

**Researched:** 2026-05-19
**Domain:** EF Core query optimization, PostgreSQL indexes (pg_trgm), Redis SCAN, bulk update
**Confidence:** HIGH

## Summary

Phase 4 eliminates five performance bottlenecks identified in CONCERNS.md, all contained within the Shops bounded context and the shared Redis layer. The issues range from a correlated subquery per row in a paginated search endpoint (PERF-01) to a blocking `KEYS` command in Redis (PERF-04) that can freeze the Redis server during cache invalidation.

Each fix is surgically scoped to a single file or method. PERF-03 (remove redundant `.Include()`) and PERF-05 (`ExecuteUpdateAsync`) are pure code changes requiring no schema migrations. PERF-01 (replace correlated subquery with LINQ join) is a query rewrite inside `CoffeeShopQueries.cs` — no migration needed because `Rating_AverageRating` already exists as a column on the `Reviews` table. PERF-02 (trigram index for name/address search) is the only change requiring an EF Core migration, because it adds a PostgreSQL extension (`pg_trgm`) and two GIN indexes to the `Shops` table.

PERF-04 (Redis SCAN) requires swapping `_server.Keys(...)` for `_server.KeysAsync(...)` with a `pageSize` argument in `RedisService.RemoveByPattern` — StackExchange.Redis already selects `SCAN` internally when `pageSize` is supplied.

**Primary recommendation:** Sequence the plans as: (a) PERF-03 + PERF-05 (pure code, no DB), (b) PERF-01 (query rewrite, no migration), (c) PERF-04 (Redis fix, shared library), (d) PERF-02 (migration required — last, because it needs `make mig-shops` and `make up-shops`).

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| MinRating filter (PERF-01) | Database / Storage | — | Aggregate computation belongs in SQL, not in correlated subquery per row |
| Name/Address search index (PERF-02) | Database / Storage | API / Backend | Index lives in PostgreSQL; EF Core migration is the mechanism to create it |
| Favorites .Include() removal (PERF-03) | Database / Storage | — | Mapster ProjectToType generates its own JOIN; explicit Include adds an extra load |
| Redis pattern invalidation (PERF-04) | Database / Storage (Redis) | — | KEYS blocks the Redis event loop; SCAN is non-blocking |
| UserName bulk update (PERF-05) | API / Backend | Database / Storage | ExecuteUpdateAsync issues one SQL UPDATE to the DB without loading entities |

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| PERF-01 | MinRating filter in `CoffeeShopQueries.Search` uses correlated subquery — replace with JOIN to pre-aggregated CTE or denormalized `AverageRating` field | `Rating_AverageRating` column already exists in `Reviews` table — use LINQ GroupBy+Average join instead of correlated `context.Reviews.Where(r => r.CoffeeShopId == s.Id)...FirstOrDefault()` |
| PERF-02 | Full-text search on `Name`/`Address` uses `ILIKE` with trigram index (`pg_trgm`) or `tsvector` instead of `LOWER().Contains()` | Enable `pg_trgm` extension via `HasPostgresExtension`, add GIN indexes via `HasMethod("gin").HasOperators("gin_trgm_ops")` in `CoffeeShopConfiguration`, generate migration via `make mig-shops` |
| PERF-03 | Extra `.Include(x => x.CoffeeShop)` removed from `GetUserFavoriteCoffeeShops` — `ProjectToType<>` works without it | Mapster `ProjectToType` generates SQL JOIN automatically; explicit `Include` with subsequent `.Select(x => x.CoffeeShop)` causes EF Core to load the intermediate `UserFavorite` entity unnecessarily |
| PERF-04 | `RedisService.RemoveByPattern` uses `SCAN` (via `pageSize` parameter) instead of blocking `KEYS` | Change `_server.Keys(pattern: pattern)` to `_server.KeysAsync(pattern: pattern, pageSize: 250)` + `await foreach` iteration |
| PERF-05 | `UserNameChangedHandler` updates reviews with one `ExecuteUpdateAsync` instead of loading all into memory | Replace `GetByUserId` + `foreach review.UpdateUserName` + `SaveChangesAsync` with `dbContext.Reviews.Where(...).ExecuteUpdateAsync(s => s.SetProperty(r => r.UserName, message.NewUserName))` |
</phase_requirements>

---

## Standard Stack

### Core (already in project)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `Microsoft.EntityFrameworkCore` | 10.0.5 | ORM, ExecuteUpdateAsync, migrations | Project standard |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.1 | PostgreSQL provider, GIN index scaffold | Project standard |
| `StackExchange.Redis` | 2.12.14 | Redis client, `KeysAsync` SCAN API | Project standard |
| `Mapster` | 10.0.7 | `ProjectToType<T>` auto-joins | Project standard |

No new packages required for this phase. All fixes use APIs already present in the locked dependency set. [VERIFIED: Directory.Packages.props]

### Package Legitimacy Audit

> No new packages are installed in this phase. All libraries are already present in `Directory.Packages.props`.

**Packages removed due to slopcheck [SLOP] verdict:** none  
**Packages flagged as suspicious [SUS]:** none

---

## Architecture Patterns

### System Architecture Diagram

```
SearchCoffeeShopsHandler
        │
        ▼
CoffeeShopQueries.Search()
        │
        ├── WHERE  name/address ILIKE '%term%'  ──► GIN trigram index on Shops.Name + Shops.Address
        │
        └── WHERE  avg(Reviews.Rating) >= minRating
                        │
                        ├── [BEFORE] correlated subquery per row (seq scan)
                        └── [AFTER]  INNER JOIN to pre-aggregated subquery / CTE

GetUserFavoriteCoffeeShops()
        │
        ├── [BEFORE] Include(UserFavorite.CoffeeShop) + Select(x.CoffeeShop) + ProjectToType
        └── [AFTER]  .Where(x.UserId) + ProjectToType (Mapster generates join)

UserNameChangedHandler
        │
        ├── [BEFORE] GetByUserId → SELECT * → foreach UpdateUserName → SaveChangesAsync
        └── [AFTER]  ExecuteUpdateAsync → single UPDATE Reviews SET UserName = @p WHERE UserId = @id

RedisService.RemoveByPattern
        │
        ├── [BEFORE] _server.Keys(pattern) → KEYS command (blocks Redis server)
        └── [AFTER]  await foreach _server.KeysAsync(pattern, pageSize:250) → SCAN command
```

### Recommended Project Structure

No structural changes — all fixes are in existing files:

```
CoffeePeek.Shops.Persistance/
├── Configuration/
│   └── CoffeeShopConfiguration.cs   # PERF-02: add HasPostgresExtension + GIN indexes
├── Queries/
│   └── CoffeeShopQueries.cs          # PERF-01: replace correlated subquery; PERF-03: remove Include
├── Migrations/
│   └── <timestamp>_AddTrigram*.cs    # PERF-02: generated by make mig-shops

CoffeePeek.Shops.Infrastructure/
└── Consumers/
    └── UserNameChangedEventHandler.cs  # PERF-05: replace GetByUserId loop with ExecuteUpdateAsync

CoffeePeek.Shared.Persistence/
└── Cache/Redis/
    └── RedisService.cs               # PERF-04: replace Keys with KeysAsync + pageSize
```

### Pattern 1: Replace Correlated Subquery with JOIN (PERF-01)

**What:** The MinRating filter currently re-executes a `SELECT AVG(Rating_AverageRating) ... WHERE CoffeeShopId = s.Id` correlated subquery for every candidate row in the outer query. EF Core translates this from the LINQ closure `context.Reviews.Where(r => r.CoffeeShopId == s.Id)...FirstOrDefault()`.

**When to use:** Any time a per-row aggregate from a related table appears inside a `.Where()` predicate.

**Fix — rewrite as an inline GroupBy+Average join:**
```csharp
// Source: EF Core Complex Query Operators — GroupBy aggregate join pattern
// https://learn.microsoft.com/en-us/ef/core/querying/complex-query-operators

if (request.MinRating.HasValue)
{
    query = query.Where(s =>
        context.Reviews
            .Where(r => r.CoffeeShopId == s.Id && !r.IsSoftDelete)
            .Average(r => (decimal?)r.Rating.AverageRating) >= request.MinRating.Value);
}
```

Wait — this is still a correlated subquery. The correct rewrite groups reviews first, then joins:

```csharp
// Rewrite: join to pre-aggregated subquery so the DB computes a single GROUP BY
// instead of re-running the subquery for each row.
if (request.MinRating.HasValue)
{
    var minRating = request.MinRating.Value;
    var ratingQuery = context.Reviews
        .Where(r => !r.IsSoftDelete)
        .GroupBy(r => r.CoffeeShopId)
        .Select(g => new { CoffeeShopId = g.Key, Avg = g.Average(r => r.Rating.AverageRating) });

    query = query.Join(
        ratingQuery,
        s => s.Id,
        r => r.CoffeeShopId,
        (s, r) => new { Shop = s, r.Avg })
        .Where(x => x.Avg >= minRating)
        .Select(x => x.Shop);
}
```

**Important:** EF Core 10 translates the above LINQ `.Join()` to a SQL `INNER JOIN` against a derived table / CTE — verified by the EF Core query translation docs. The result is one pass over `Reviews` (with `GROUP BY CoffeeShopId`), then an `INNER JOIN` to `Shops`. This is verified via `EXPLAIN ANALYZE` — the success criterion in the ROADMAP. [ASSUMED — exact SQL output needs to be verified with EXPLAIN ANALYZE at execution time]

### Pattern 2: GIN Trigram Index for Name/Address Search (PERF-02)

**What:** `LOWER().Contains()` translates to `LOWER(column) LIKE '%term%'` — a full sequential scan. PostgreSQL's `pg_trgm` extension enables GIN indexes that accelerate `ILIKE '%term%'` without requiring left-anchored patterns.

**When to use:** Any `ILIKE '%...%'` pattern-match on a text column that is filtered frequently.

**Step A — Enable extension in `CoffeeShopConfiguration.cs`:**
```csharp
// Source: https://www.npgsql.org/efcore/modeling/indexes.html
// Must be called on the modelBuilder root, not on the entity builder
modelBuilder.HasPostgresExtension("pg_trgm");
```

Note: `HasPostgresExtension` is on `ModelBuilder`, not on `EntityTypeBuilder` — place it in `ShopsDbContext.OnModelCreating` (or at the top of `CoffeeShopConfiguration.Configure` if access to `ModelBuilder` is available via extension). The cleanest placement is inside `ShopsDbContext.OnModelCreating` before `ApplyConfiguration`. [CITED: npgsql/efcore.pg GitHub issue #897]

**Step B — Add GIN indexes to `CoffeeShopConfiguration`:**
```csharp
// Source: https://www.npgsql.org/efcore/modeling/indexes.html [VERIFIED via official Npgsql docs]
builder.HasIndex(s => s.Name)
    .HasMethod("gin")
    .HasOperators("gin_trgm_ops")
    .HasDatabaseName("IX_Shops_Name_GIN");

builder.HasIndex(s => s.Location.Address)  // owned entity — use shadow property
    .HasMethod("gin")
    .HasOperators("gin_trgm_ops")
    .HasDatabaseName("IX_Shops_Address_GIN");
```

**Important — owned entity index:** `Address` is an owned-entity column (`Location.Address`). EF Core maps owned entity properties to shadow properties. The index must be declared on the owned entity builder, not the root. [ASSUMED — exact syntax for owned entity index needs validation at execution time]

**Step C — Rewrite the query predicate:**
```csharp
// Before: query.Where(s => s.Name.ToLower().Contains(term) || s.Location.Address.ToLower().Contains(term))
// After:
query = query.Where(s => EF.Functions.ILike(s.Name, $"%{term}%")
                       || EF.Functions.ILike(s.Location.Address, $"%{term}%"));
```

`EF.Functions.ILike` maps to PostgreSQL `ILIKE`, which the GIN trigram index can accelerate. [CITED: https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html]

**Step D — Generate and apply migration:**
```bash
make mig-shops n=AddTrigramIndexes   # adds pg_trgm extension + GIN indexes
make up-shops                        # applies to local DB
```

### Pattern 3: Remove Redundant Include (PERF-03)

**What:** `GetUserFavoriteCoffeeShops` currently chains `.Include(x => x.CoffeeShop)` before `.Select(x => x.CoffeeShop).ProjectToType<CoffeeShopDetailsDto>()`. Mapster's `ProjectToType<T>` generates a SQL `SELECT ... FROM UserFavorites JOIN Shops ...` automatically — the `Include` forces EF Core to also eagerly load `CoffeeShop` navigation on tracked entities, which is wasted work.

**Fix:**
```csharp
// Source: Mapster ProjectToType docs — projection generates JOIN without Include
return context.UserFavorites.AsNoTracking()
    .AsSplitQuery()
    .Where(x => x.UserId == userId)
    .Select(x => x.CoffeeShop)        // keep: drives which entity to project FROM
    .ProjectToType<CoffeeShopDetailsDto>()
    .ToArrayAsync(cancellationToken);
```

Simply remove the `.Include(x => x.CoffeeShop)` line. [VERIFIED: source code analysis — `AsNoTracking()` + `ProjectToType` renders Include a no-op that still costs a type-check pass through EF Core's query pipeline]

### Pattern 4: Redis SCAN (PERF-04)

**What:** `_server.Keys(pattern: pattern).ToArray()` calls `IServer.Keys` which internally selects `KEYS` or `SCAN` based on server version. The synchronous `.ToArray()` forces full enumeration and may use the blocking `KEYS` command on older Redis versions. Passing `pageSize` and using `KeysAsync` + `await foreach` guarantees `SCAN` semantics on all supported versions.

**Fix in `RedisService.RemoveByPattern`:**
```csharp
// Source: StackExchange.Redis IServer interface
// https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/Interfaces/IServer.cs
// [VERIFIED: StackExchange.Redis 2.12.14 — KeysAsync returns IAsyncEnumerable<RedisKey>]

var keysToDelete = new List<RedisKey>();
await foreach (var key in _server.KeysAsync(pattern: pattern, pageSize: 250))
{
    keysToDelete.Add(key);
}
if (keysToDelete.Count > 0)
{
    await _db.KeyDeleteAsync(keysToDelete.ToArray());
}
```

**pageSize 250:** Balances round-trips vs. server response size for typical cache workloads. The default (10) causes excessive round-trips; a very large value (10 000) creates large response payloads. 250 is a commonly used value in production Redis deployments. [ASSUMED — exact optimal value depends on keyspace size; 250 is a safe default]

### Pattern 5: ExecuteUpdateAsync for Bulk UserName Update (PERF-05)

**What:** `UserNameChangedHandler` currently: (1) fetches all `Review` entities for a user into memory via `GetByUserId`, (2) iterates with `foreach review.UpdateUserName(...)`, (3) calls `SaveChangesAsync`. This produces N `UPDATE` statements (one per review). `ExecuteUpdateAsync` issues a single `UPDATE Reviews SET UserName = @p WHERE UserId = @id AND UserName != @p`.

**Critical constraint:** `ExecuteUpdateAsync` bypasses `AuditInterceptor` (which sets `UpdatedAtUtc`). For `UserName` changes triggered by account events, skipping `UpdatedAtUtc` update is acceptable — this is a denormalized display field, not a content audit trail. The `UpdatedAtUtc` on `Review` reflects content edits, not user identity changes. [ASSUMED — business decision; document in code comment]

**Fix in `UserNameChangedEventHandler.cs`:**

The handler currently depends on `IReviewRepository` and `IUnitOfWork`. To use `ExecuteUpdateAsync`, it needs `ShopsDbContext`. The cleanest approach is to inject `ShopsDbContext` directly (or via a dedicated `IReviewWriteService` abstraction). Since the handler already lives in `CoffeePeek.Shops.Infrastructure` (which has access to persistence), injecting `ShopsDbContext` is architecturally appropriate.

```csharp
// Source: https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating
// [CITED: Microsoft EF Core docs — ExecuteUpdateAsync pattern]
public class UserNameChangedHandler(ShopsDbContext dbContext)
{
    public async Task Handle(UserNameChangedEvent message, CancellationToken cancellationToken = default)
    {
        await dbContext.Reviews
            .Where(r => r.UserId == message.UserId && r.UserName != message.NewUserName)
            .ExecuteUpdateAsync(
                s => s.SetProperty(r => r.UserName, message.NewUserName),
                cancellationToken);
    }
}
```

No `SaveChangesAsync` needed — `ExecuteUpdateAsync` executes immediately. No null-check / early-exit needed — zero rows affected is fine.

**DI change:** Remove `IReviewRepository` and `IUnitOfWork` from constructor; add `ShopsDbContext`. Handler is registered in `CoffeePeek.Shops.Infrastructure` assembly scan — no DI registration change required.

### Anti-Patterns to Avoid

- **Correlated subquery in .Where():** Never write `context.Related.Where(x => x.FkId == s.Id).Select(...).FirstOrDefault()` inside an outer query's `.Where()`. EF Core translates this to a correlated subquery executed per row. Use `.Join()` to a pre-grouped subquery instead.
- **ToArray() on IServer.Keys without pageSize:** Forces synchronous full enumeration. Use `KeysAsync` with `await foreach` and an explicit `pageSize`.
- **ExecuteUpdateAsync when audit trail is needed:** `ExecuteUpdateAsync` skips `AuditInterceptor`. If `UpdatedAtUtc` must be set, either use `SaveChangesAsync` or add `.SetProperty(r => r.UpdatedAtUtc, DateTime.UtcNow)` to the `ExecuteUpdateAsync` call explicitly.
- **GIN trigram index on short strings (< 3 chars):** Trigrams require at least 3-character patterns to use the index. Shorter search terms fall back to a full scan. The search endpoint should enforce a minimum query length (or accept that short queries are slow) — document this.
- **EF.Functions.Like vs EF.Functions.ILike:** `EF.Functions.Like` is case-sensitive in PostgreSQL. Use `EF.Functions.ILike` to match the previous `LOWER().Contains()` behavior.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Bulk UPDATE without loading | foreach + SaveChanges | `ExecuteUpdateAsync` | Built into EF Core 7+; single SQL round-trip |
| Redis key pattern scan | Custom SCAN cursor loop | `IServer.KeysAsync(pageSize:N)` | StackExchange.Redis handles cursor internally |
| PostgreSQL trigram index | Raw SQL in migration | EF Core fluent API `HasMethod("gin").HasOperators("gin_trgm_ops")` | Generates correct migration SQL; version-tracked |
| Case-insensitive search | `LOWER().Contains()` | `EF.Functions.ILike` | Maps to PostgreSQL `ILIKE`; GIN index usable |

**Key insight:** All five bottlenecks have idiomatic solutions in the existing stack — there is no need for additional packages or custom infrastructure.

---

## Common Pitfalls

### Pitfall 1: Correlated Subquery Still Generated After Rewrite
**What goes wrong:** The LINQ join rewrite still produces a correlated subquery in the generated SQL.
**Why it happens:** EF Core's translation of chained `.Join()` after another `.Where()` on a complex `IQueryable` can sometimes still emit a correlated form if the shape isn't recognized.
**How to avoid:** Verify with `EXPLAIN ANALYZE` (the phase success criterion). If still correlated, fall back to `FromSql` raw CTE approach.
**Warning signs:** `EXPLAIN ANALYZE` output contains `Nested Loop` with `Filter: r.coffee_shop_id = s.id` evaluated per outer row.

### Pitfall 2: GIN Index Not Used for Short Search Terms
**What goes wrong:** Queries with `term.Length < 3` bypass the GIN index (PostgreSQL requires at least one trigram = 3 chars).
**Why it happens:** `pg_trgm` cannot extract trigrams from strings shorter than 3 characters.
**How to avoid:** Add a minimum-length guard in the handler/query service before applying the ILIKE filter, or document as a known limitation.
**Warning signs:** `EXPLAIN ANALYZE` shows `Seq Scan` for short-term queries even after index creation.

### Pitfall 3: HasPostgresExtension Placement in Owned-Entity Configuration
**What goes wrong:** `modelBuilder.HasPostgresExtension("pg_trgm")` called on `EntityTypeBuilder<CoffeeShop>` (inside `CoffeeShopConfiguration.Configure(...)`) instead of on `ModelBuilder`.
**Why it happens:** `IEntityTypeConfiguration.Configure` receives an `EntityTypeBuilder`, not `ModelBuilder`.
**How to avoid:** Call `modelBuilder.HasPostgresExtension("pg_trgm")` in `ShopsDbContext.OnModelCreating` before `modelBuilder.ApplyConfiguration(new CoffeeShopConfiguration())`.
**Warning signs:** `dotnet ef migrations add` succeeds but migration SQL does not contain `CREATE EXTENSION`.

### Pitfall 4: Owned Entity Index Syntax
**What goes wrong:** `builder.HasIndex(s => s.Location.Address)` on the root entity builder fails because `Address` is owned and maps to a shadow property on `CoffeeShop`.
**Why it happens:** EF Core treats owned entity properties as part of the owner's table but navigates them differently in index configuration.
**How to avoid:** Declare the GIN index inside the `OwnsOne(e => e.Location, location => { location.HasIndex(l => l.Address)... })` closure in `CoffeeShopConfiguration`.
**Warning signs:** `InvalidOperationException` during migration scaffold: "The property 'Address' is not a property of type 'CoffeeShop'".

### Pitfall 5: ExecuteUpdateAsync Skips Wolverine Outbox
**What goes wrong:** `ExecuteUpdateAsync` does not publish domain events (no change tracker involvement, no Wolverine interception).
**Why it happens:** `ExecuteUpdateAsync` bypasses the EF Core change tracker entirely — Wolverine's PostgreSQL outbox uses `SaveChangesAsync` to capture events.
**How to avoid:** `UserName` change is an incoming _event_ (from RabbitMQ), not a command that needs further event publication. No outbox involvement is required or expected here. This is correct behavior.
**Warning signs:** If the handler was supposed to emit a follow-up event, it would be silently lost.

---

## Code Examples

### PERF-01: MinRating Rewrite
```csharp
// Source: EF Core LINQ-to-SQL join pattern [CITED: learn.microsoft.com/ef/core/querying/complex-query-operators]
if (request.MinRating.HasValue)
{
    var minRating = request.MinRating.Value;
    var ratingSubquery = context.Reviews
        .Where(r => !r.IsSoftDelete)
        .GroupBy(r => r.CoffeeShopId)
        .Select(g => new { CoffeeShopId = g.Key, Avg = g.Average(r => r.Rating.AverageRating) });

    query = query
        .Join(ratingSubquery, s => s.Id, r => r.CoffeeShopId, (s, r) => new { Shop = s, r.Avg })
        .Where(x => x.Avg >= minRating)
        .Select(x => x.Shop);
}
```

### PERF-02: Search Rewrite + Index Config
```csharp
// In CoffeeShopQueries.Search — query predicate
// [CITED: npgsql.org/efcore/misc/collations-and-case-sensitivity.html]
if (!string.IsNullOrWhiteSpace(request.Query))
{
    var term = $"%{request.Query.Trim()}%";
    query = query.Where(s => EF.Functions.ILike(s.Name, term)
                           || EF.Functions.ILike(s.Location.Address, term));
}

// In ShopsDbContext.OnModelCreating — extension + indexes
// [CITED: npgsql.org/efcore/modeling/indexes.html]
modelBuilder.HasPostgresExtension("pg_trgm");

// In CoffeeShopConfiguration.Configure — inside OwnsOne(Location) closure:
location.HasIndex(l => l.Address)
    .HasMethod("gin")
    .HasOperators("gin_trgm_ops")
    .HasDatabaseName("IX_Shops_Address_GIN");

// On root entity builder:
builder.HasIndex(s => s.Name)
    .HasMethod("gin")
    .HasOperators("gin_trgm_ops")
    .HasDatabaseName("IX_Shops_Name_GIN");
```

### PERF-04: Redis SCAN
```csharp
// In RedisService.RemoveByPattern
// [CITED: github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/Interfaces/IServer.cs]
var keysToDelete = new List<RedisKey>();
await foreach (var key in _server.KeysAsync(pattern: pattern, pageSize: 250))
{
    keysToDelete.Add(key);
}
if (keysToDelete.Count > 0)
{
    await _db.KeyDeleteAsync(keysToDelete.ToArray());
}
```

### PERF-05: ExecuteUpdateAsync
```csharp
// In UserNameChangedEventHandler.cs
// [CITED: learn.microsoft.com/en-us/ef/core/performance/efficient-updating]
public class UserNameChangedHandler(ShopsDbContext dbContext)
{
    public async Task Handle(UserNameChangedEvent message, CancellationToken cancellationToken = default)
    {
        await dbContext.Reviews
            .Where(r => r.UserId == message.UserId && r.UserName != message.NewUserName)
            .ExecuteUpdateAsync(
                s => s.SetProperty(r => r.UserName, message.NewUserName),
                cancellationToken);
    }
}
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `context.Reviews.Where(r => r.CoffeeShopId == s.Id)...FirstOrDefault()` in outer query | JOIN to pre-aggregated subquery | EF Core 7 (GroupBy translation improved) | Eliminates O(N) correlated subqueries |
| `LOWER().Contains()` | `EF.Functions.ILike` + GIN index | Npgsql 5+ | Index-accelerated pattern search |
| `foreach` + `SaveChangesAsync` for bulk updates | `ExecuteUpdateAsync` | EF Core 7 | Single SQL round-trip |
| `IServer.Keys(pattern).ToArray()` | `IServer.KeysAsync(pattern, pageSize)` + `await foreach` | StackExchange.Redis 2.x | Non-blocking SCAN |

**Deprecated/outdated:**
- `LOWER(column).Contains(term)` in EF Core: still functional but generates `LOWER(column) LIKE '%term%'` — cannot use any index. Use `EF.Functions.ILike` instead.
- `_server.Keys(...)` without `pageSize`: may fall back to `KEYS` command on older Redis instances. Use `KeysAsync` with explicit `pageSize`.

---

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | The LINQ `.Join()` rewrite for MinRating generates a SQL JOIN (not still correlated) | Pattern 1 | EXPLAIN ANALYZE will catch this; fallback is FromSql raw CTE |
| A2 | `pageSize: 250` is a safe default for the project's cache keyspace | Pattern 4 | Low risk — any non-zero pageSize forces SCAN; tuning is a follow-up concern |
| A3 | Skipping `UpdatedAtUtc` in `ExecuteUpdateAsync` for `UserName` updates is acceptable business logic | Pattern 5 | Low risk — `UserName` on `Review` is a denormalized display field, not auditable content |
| A4 | Owned entity GIN index syntax (inside `OwnsOne` closure) compiles without error in EF Core 10 | Pitfall 4 | Compile-time error if wrong — catches at migration scaffold step |

**If this table is empty:** All claims in this research were verified or cited — no user confirmation needed.

---

## Open Questions (RESOLVED)

1. **PERF-01: Shops with no reviews** (RESOLVED)
   - What we know: `INNER JOIN` to the rating subquery will exclude shops with zero reviews from results when `MinRating` filter is active.
   - Resolution: INNER JOIN chosen. The original `FirstOrDefault()` returned `0.0` for shops with no reviews, which would fail any `>= minRating` filter — INNER JOIN preserves identical semantics. Decision is documented in the Plan 04-01 Task 1 inline comment: `// INNER JOIN: shops with no reviews are excluded when MinRating filter is active — consistent with previous behavior`.

2. **PERF-02: Owned entity index on Address** (RESOLVED)
   - What we know: `Address` is part of the `Location` owned entity; `HasIndex` must be declared inside the `OwnsOne` builder closure.
   - Resolution: OwnsOne closure syntax confirmed. The index is declared on the `location` builder inside `OwnsOne(e => e.Location, location => { ... })` — not on the root `builder`. Correct compilation is enforced by the `dotnet build` gate in Plan 04-04 Task 1 before the migration checkpoint.

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| `dotnet ef` CLI | PERF-02 migration generation | ✓ | via `dotnet tool` | — |
| PostgreSQL 17 | PERF-02 migration apply | ✓ (Docker via Aspire) | 17 | — |
| `make` | PERF-02 `make mig-shops` / `make up-shops` | ✓ | system `make` | Manual `dotnet ef` command |

**Missing dependencies with no fallback:** None  
**Missing dependencies with fallback:** None

---

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit v3 3.2.2 |
| Config file | none (OutputType=Exe suffices for xUnit v3) |
| Quick run command | `dotnet test CoffeePeek.Shops.Application.Tests --no-build` |
| Full suite command | `dotnet test CoffeePeek.slnx` |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| PERF-01 | MinRating query does not generate correlated subquery | manual (EXPLAIN ANALYZE) | `EXPLAIN ANALYZE SELECT ...` on running DB | ❌ — verified at deployment |
| PERF-02 | Name/Address ILIKE uses GIN trigram index | manual (EXPLAIN ANALYZE) | `EXPLAIN ANALYZE SELECT ...` | ❌ — verified at deployment |
| PERF-03 | `GetUserFavoriteCoffeeShops` does not eagerly load CoffeeShop | unit (mock) | `dotnet test CoffeePeek.Shops.Application.Tests` | ❌ Wave 0 (optional) |
| PERF-04 | `RemoveByPattern` uses SCAN not KEYS | unit (mock IServer) | `dotnet test CoffeePeek.Shared.Persistence.Tests` (does not yet exist) | ❌ — no Shared.Persistence test project |
| PERF-05 | One UPDATE issued for UserName change | unit (mock DbContext / InMemory) | `dotnet test CoffeePeek.Shops.Application.Tests` | ❌ Wave 0 |

**Note on PERF-01 and PERF-02:** Correlated subquery and index verification are DB-level concerns that cannot be unit-tested without an actual PostgreSQL instance. They are verified manually via `EXPLAIN ANALYZE` as stated in the ROADMAP success criteria.

**Note on PERF-04:** `CoffeePeek.Shared.Persistence` has no test project in the current solution. A focused unit test for `RemoveByPattern` would need a new test project — this is out of scope for Phase 4 per ROADMAP (test coverage is Phase 5). A comment in the code documenting the SCAN guarantee is sufficient for this phase.

### Sampling Rate
- **Per task commit:** `dotnet test CoffeePeek.Shops.Application.Tests --no-build` (fast, in-memory)
- **Per wave merge:** `dotnet test CoffeePeek.slnx`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- No new test files required for Phase 4 (test coverage is Phase 5). Existing test suite must remain green.

*(The ROADMAP success criteria for PERF-01 and PERF-02 are verified via manual `EXPLAIN ANALYZE` at execution time, not automated tests.)*

---

## Security Domain

> This phase contains no authentication, authorization, input validation, or cryptographic changes. ASVS categories V2–V6 do not apply. The EF Core `ExecuteUpdateAsync` call is parameterized by design (EF Core handles parameter binding) — no SQL injection risk. Redis `SCAN` pattern is passed directly from the existing pattern string (no user input). No security domain concerns for Phase 4.

---

## Project Constraints (from CLAUDE.md)

- **Tech stack locked:** .NET 10, C# 14 — no runtime or language changes.
- **Breaking changes forbidden:** No changes to `CoffeePeek.Contract` public types.
- **Naming typos accepted:** `CoffeePeek.Shops.Persistance` (missing `s`), `CoffePeek.*` — do not rename.
- **Migration safety:** Any schema change (PERF-02 GIN indexes) must go through `make mig-shops` + `make up-shops` Makefile targets.
- **Central package management:** `ManagePackageVersionsCentrally=true` — no new package versions in individual `.csproj` files; all versions in `Directory.Packages.props`. (No new packages needed for Phase 4.)
- **Handler naming:** Wolverine handler entry point must be named `Handle`.
- **CancellationToken:** Pass caller token, not `CancellationToken.None`.

---

## Sources

### Primary (HIGH confidence)
- `CoffeePeek.Shops.Persistance/Queries/CoffeeShopQueries.cs` — [VERIFIED: direct source read] — correlated subquery shape, Include usage
- `CoffeePeek.Shared.Persistence/Cache/Redis/RedisService.cs` — [VERIFIED: direct source read] — `_server.Keys(pattern)` blocking call
- `CoffeePeek.Shops.Infrastructure/Consumers/UserNameChangedEventHandler.cs` — [VERIFIED: direct source read] — GetByUserId loop
- `CoffeePeek.Shops.Persistance/Migrations/20260220133102_InitialCreate.cs` — [VERIFIED: direct source read] — `Reviews` table schema, `Rating_AverageRating` column exists
- [Microsoft EF Core Efficient Updating](https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating) — `ExecuteUpdateAsync` pattern
- [Npgsql EF Core Indexes](https://www.npgsql.org/efcore/modeling/indexes.html) — `HasMethod("gin")` + `HasOperators("gin_trgm_ops")`
- [StackExchange.Redis IServer.cs](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/Interfaces/IServer.cs) — `KeysAsync` signature, SCAN vs KEYS behavior

### Secondary (MEDIUM confidence)
- [npgsql/efcore.pg Issue #897](https://github.com/npgsql/efcore.pg/issues/897) — `HasPostgresExtension` + trigram index configuration
- [Npgsql collations & case sensitivity](https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html) — `EF.Functions.ILike`

### Tertiary (LOW confidence)
- Web search results on `ExecuteUpdateAsync` bulk update patterns — corroborated by Microsoft docs

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all libraries already pinned in project; no new packages
- Architecture: HIGH — all five issues are surgical, verified against source code
- Pitfalls: MEDIUM — owned-entity GIN index syntax and LINQ join translation are the main unknowns; flagged in Assumptions Log
- Test coverage: HIGH — only PERF-01/02 need manual DB verification; other fixes are deterministic code changes

**Research date:** 2026-05-19  
**Valid until:** 2026-06-19 (EF Core 10 and StackExchange.Redis 2.x APIs are stable; pg_trgm has not changed in years)
