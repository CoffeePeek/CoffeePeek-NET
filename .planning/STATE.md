# State

## Quick Tasks Completed

| ID | Slug | Status | Date |
|----|------|--------|------|
| 260801-erp | remove-community-social-layer-feed-posts | complete | 2026-08-01 |

## Milestone Progress

**Tier 0** waves **A–E** are done (security, account/auth, admin sessions, password reset, check-ins/favorites cleanup, related tests).

## Specs / next

- Shop filters & tags SPEC written at `.planning/specs/SHOP-FILTERS-TAGS.md`.

## Accumulated Context

### Exploration (2026-08-15)

Specialty coffee shop bootstrap for Minsk: OSM + lists → admin review queue → catalog. See note `2026-08-15-specialty-coffee-shops-minsk-import`, seed `SEED-001`.

Spike 001 VALIDATED: Overpass Minsk = 1576 candidates (priority ~380, vending 96). OSM is not a specialty catalog. Draft aggregate: `ShopImportCandidate`, not `ModerationShop`. Import labels **ShopKind** (Specialty / GoodCoffee / Cafe / ToGo); reject stays out of the feed. Review card must show Instagram + Yandex photos/images + website preview — coordinates alone are not enough.

### Pending Todos

_None._
