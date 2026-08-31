# State

## Quick Tasks Completed

| ID | Slug | Status | Date |
|----|------|--------|------|
| 260801-erp | remove-community-social-layer-feed-posts | complete | 2026-08-01 |
| 260815-fast | widen-import-candidate-phone | complete | 2026-08-15 |
| 260815-fast | admin-overview-always-returns | complete | 2026-08-15 |
| 260831-krk | add-full-crud-create-update-delete-for-c | complete | 2026-08-31 |

## Milestone Progress

**Tier 0** waves **A–E** are done (security, account/auth, admin sessions, password reset, check-ins/favorites cleanup, related tests).

## Specs / next

- Shop filters & tags SPEC written at `.planning/specs/SHOP-FILTERS-TAGS.md`.
- Catalog ingest + admin queue SPEC: `.planning/specs/SHOP-IMPORT-ADMIN.md` (backend vs frontend handoff).
- Coffee shop menu (catalog drinks + Gemini photo parse): `.planning/specs/SHOP-MENU.md`.

## Accumulated Context

### Exploration (2026-08-15)

Specialty coffee shop bootstrap for Minsk: OSM + lists → admin review queue → catalog. See note `2026-08-15-specialty-coffee-shops-minsk-import`, seed `SEED-001`.

Spike 001 VALIDATED: Overpass Minsk = 1576 candidates (priority ~380, vending 96). OSM is not a specialty catalog. Draft aggregate: `ShopImportCandidate`, not `ModerationShop`. Import labels **ShopKind** (Specialty / GoodCoffee / Cafe / ToGo); reject stays out of the feed. Review card must show Instagram + Yandex photos/images + website preview — coordinates alone are not enough. OSM objects not edited in 5+ years are dropped from the queue.

### Pending Todos

_None._
