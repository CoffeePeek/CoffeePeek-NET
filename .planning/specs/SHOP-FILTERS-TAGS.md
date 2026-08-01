# SPEC: Admin-managed shop filters / tags

**Status:** design only — not implemented  
**Depends on:** Tier 0 complete (favorites removed; discovery search stable)  
**Out of scope here:** Favorite-as-filter, client UI

## Problem

Legacy client chips (Open / New / Favorite / Visited) mixed three different concepts:

| Chip | Nature | Who sets it |
|------|--------|-------------|
| Favorite | Per-user preference | User (removed from backend) |
| Visited | Derived from check-ins | System |
| New | Derived from shop age | System |
| Open | Derived from schedules | System |
| Specialty / laptop / pet / … | Curated attributes | **Missing** — need admin (and maybe owner) |

Hardcoding more chips without a catalog will not scale. Filters must be data-driven.

## Goals

1. Admin can create/edit/deactivate **filter tags** (global catalog).
2. Admin (and optionally owner via moderation later) can **assign tags to shops**.
3. Public search accepts `tags` query param alongside existing catalog filters.
4. Computed filters stay separate from curated tags (no Favorite on server).

## Non-goals (v1 of this feature)

- User-created tags
- Multi-language tag UI beyond `Name` + optional `NameRu`
- Owner self-serve tag assignment without moderation (phase 2)
- Map clustering / PostGIS

## Domain model (Shops)

```
ShopTag (catalog)
  Id: Guid
  Slug: string          // laptop_friendly, specialty, pet_friendly
  Name: string
  Description?: string
  SortOrder: int
  IsActive: bool
  CreatedAtUtc / UpdatedAtUtc

CoffeeShopTag (join)
  ShopId: Guid
  TagId: Guid
  AssignedByUserId: Guid
  AssignedAtUtc: DateTime
```

Seed optional starter tags via migration or admin seed (not hardcoded in search handler).

## API sketch

### Admin (Authorization: Admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/admin/shop-tags` | List all tags (incl. inactive) |
| POST | `/api/admin/shop-tags` | Create tag |
| PATCH | `/api/admin/shop-tags/{id}` | Update name/slug/sort/active |
| DELETE | `/api/admin/shop-tags/{id}` | Soft-deactivate (prefer) or hard-delete if unused |
| PUT | `/api/admin/shops/{shopId}/tags` | Replace tag set on shop |

### Public / catalogs

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/Catalogs/shop-tags` | Active tags only (for filter UI) |

### Search

Extend `SearchCoffeeShopsQuery`:

```
Guid[]? Tags = null   // AND semantics by default: shop must have all listed tags
bool? IsOpen = null   // computed from schedules at query time
bool? IsNew = null    // CreatedAt within BusinessConstants.ItNewEntityInDays
bool? IsVisited = null // only when UserId present; filter to shops user checked into
```

**AND vs OR for tags:** v1 = AND (narrowing). Document clearly; OR can be a later `tagMode` if needed.

## Computed filters (not tags)

Implement as query flags, not `ShopTag` rows:

- **IsOpen** — evaluate `Schedules` against UTC/local city time in query layer (must fix list Mapster stub that maps `IsOpen => false`).
- **IsNew** — `CreatedAtUtc >= UtcNow - N days`.
- **IsVisited** — join/exists on CheckIns for current user; ignore flag if anonymous.

## Who assigns tags

| Actor | v1 | Later |
|-------|----|-------|
| Admin | Yes — assign on published shops | — |
| Owner | No | Suggest tags → moderation queue |
| Moderator | No | Approve owner suggestions |
| System | No for curated tags | Auto-`new_arrival` optional job — prefer computed IsNew instead |

## Gateway

- Route `/api/admin/shop-tags` under existing shops-admin or dedicated admin shops cluster with Admin policy.
- Catalogs route already on shops cluster.

## Caching

- Catalog tags: short TTL or invalidate on admin write (`city:list`-style pattern `shop:tags:*`).
- Search cache key must include `tags`, `isOpen`, `isNew`, `isVisited`.

## Acceptance criteria (when implementing)

1. Admin CRUD tags; inactive tags hidden from public catalog.
2. Assigning tags to shop reflected in `GET /api/CoffeeShops?tags=...`.
3. Search with multiple tags uses AND.
4. `IsOpen` / `IsNew` / `IsVisited` work without Favorite.
5. Tests: domain join uniqueness; search filter; admin authorize.

## Open questions for `/gsd-discuss-phase`

1. Slug immutability after create?
2. Max tags per shop?
3. Should shop detail DTO return assigned tags?
4. Owner suggestion flow in same milestone or later?

## Suggested next command

`/gsd-discuss-phase` or `/gsd-spec-phase` for “Admin shop tags” once Tier 0 is merged; then `/gsd-plan-phase`.
