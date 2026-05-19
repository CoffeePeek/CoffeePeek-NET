---
phase: 04-performance-optimization
plan: "03"
subsystem: CoffeePeek.Shared.Persistence
tags: [redis, performance, caching, scan]
dependency_graph:
  requires: []
  provides: [non-blocking-redis-pattern-delete]
  affects: [CoffeePeek.Shared.Persistence]
tech_stack:
  added: []
  patterns: [Redis SCAN cursor via KeysAsync]
key_files:
  modified:
    - CoffeePeek.Shared.Persistence/Cache/Redis/RedisService.cs
decisions:
  - "Use KeysAsync(pattern, pageSize:250) with await foreach — forces Redis SCAN cursor semantics, eliminating blocking KEYS command"
  - "pageSize:250 chosen per plan spec to limit per-cursor response size"
metrics:
  duration: "~3 minutes"
  completed: "2026-05-19"
  tasks_total: 1
  tasks_completed: 1
  files_modified: 1
---

# Phase 04 Plan 03: Fix Blocking Redis KEYS Command Summary

Replaced the blocking `_server.Keys(pattern).ToArray()` call in `RedisService.RemoveByPattern` with non-blocking `_server.KeysAsync(pattern, pageSize:250)` enumerated via `await foreach`. StackExchange.Redis uses the Redis SCAN cursor command internally when `pageSize` is provided, eliminating the server-blocking KEYS anti-pattern.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Replace _server.Keys().ToArray() with KeysAsync + await foreach | 1a7d7ed | CoffeePeek.Shared.Persistence/Cache/Redis/RedisService.cs |

## Changes Made

### RedisService.RemoveByPattern

Before (blocking KEYS):
```csharp
var keys = _server.Keys(pattern: pattern).ToArray();
if (keys.Length > 0)
{
    await _db.KeyDeleteAsync(keys);
}
```

After (non-blocking SCAN):
```csharp
// SCAN: KeysAsync with pageSize forces Redis SCAN cursor (non-blocking) instead of KEYS command
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

## Verification

- `dotnet build CoffeePeek.slnx` — 0 errors
- `dotnet test CoffeePeek.slnx` — 400 tests passed, 0 failed
- `grep "KeysAsync"` in RedisService.cs — 1 match (line 190)
- `grep "_server\.Keys("` in RedisService.cs — 0 matches (blocking call removed)

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None.

## Threat Flags

None — no new network endpoints, auth paths, or schema changes introduced. The fix reduces DoS surface by eliminating the blocking KEYS command (T-04-03-01 mitigated).

## Self-Check: PASSED

- File exists: CoffeePeek.Shared.Persistence/Cache/Redis/RedisService.cs — FOUND
- Commit 1a7d7ed exists — FOUND
- KeysAsync present in RemoveByPattern — CONFIRMED
- _server.Keys( absent from file — CONFIRMED
