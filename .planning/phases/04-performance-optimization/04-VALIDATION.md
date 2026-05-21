---
phase: 4
slug: performance-optimization
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-05-19
---

# Phase 4 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit v3 3.2.2 |
| **Config file** | none (OutputType=Exe suffices for xUnit v3) |
| **Quick run command** | `dotnet test CoffeePeek.Shops.Application.Tests --no-build` |
| **Full suite command** | `dotnet test CoffeePeek.slnx` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test CoffeePeek.Shops.Application.Tests --no-build`
- **After every plan wave:** Run `dotnet test CoffeePeek.slnx`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** ~15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 04-01-01 | 01 | 1 | PERF-01 | — | N/A | manual (EXPLAIN ANALYZE) | `EXPLAIN ANALYZE SELECT ...` on running DB | ❌ verified at deployment | ⬜ pending |
| 04-01-02 | 01 | 1 | PERF-03 | — | N/A | build | `dotnet test CoffeePeek.slnx` | ✅ existing | ⬜ pending |
| 04-02-01 | 02 | 1 | PERF-05 | — | N/A | unit (mock DbContext) | `dotnet test CoffeePeek.Shops.Application.Tests` | ❌ Wave 0 | ⬜ pending |
| 04-03-01 | 03 | 1 | PERF-04 | — | N/A | code review | grep for KeysAsync usage | ✅ existing | ⬜ pending |
| 04-04-01 | 04 | 2 | PERF-02 | — | N/A | build | `dotnet build CoffeePeek.slnx` | ✅ existing | ⬜ pending |
| 04-04-02 | 04 | 2 | PERF-02 | — | N/A | manual (EXPLAIN ANALYZE) | `EXPLAIN ANALYZE SELECT ...` | ❌ verified at deployment | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- No new test files strictly required for Phase 4 (test coverage is Phase 5's scope).
- Existing test suite must remain green after all changes.

*Existing infrastructure covers all automated phase requirements.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| MinRating filter uses JOIN not correlated subquery | PERF-01 | Requires running PostgreSQL instance | `EXPLAIN ANALYZE` on shops search query with `minRating` filter — confirm no `SubPlan` node |
| Name/Address ILIKE uses GIN trigram index | PERF-02 | Requires running PostgreSQL instance with applied migration | `EXPLAIN ANALYZE` on search query — confirm `Bitmap Index Scan using ix_shops_name_trgm` node |
