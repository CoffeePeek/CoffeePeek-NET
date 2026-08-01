# SPEC: Admin-managed shop filters / tags

**Status:** implemented  
**Depends on:** Tier 0 complete (favorites removed; discovery search stable)  
**Out of scope here:** Favorite-as-filter, client UI, owner self-serve tag suggestions

## Problem

Legacy client chips (Open / New / Favorite / Visited) mixed three different concepts:

| Chip | Nature | Who sets it |
|------|--------|-------------|
| Favorite | Per-user preference | User (removed from backend) |
| Visited | Derived from check-ins | System |
| New | Derived from shop age | System |
| Open | Derived from schedules | System |
| Specialty / laptop / pet / … | Curated attributes | **Admin catalog** |

Hardcoding more chips without a catalog will not scale. Filters must be data-driven.

## Goals

1. Admin can create/edit/deactivate **filter tags** (global catalog).
2. Admin can **assign tags to shops**.
3. Public search accepts `tags` query param alongside computed filters (`isOpen`, `isNew`, `isVisited`).
4. Computed filters stay separate from curated tags (no Favorite on server).

## Locked decisions

1. **Slug immutable after create** — PATCH updates `Name`, `Description`, `SortOrder`, `IsActive` only.
2. **Max tags per shop = 20** (`BusinessConstants.MaxShopTagsPerShop`).
3. **Shop detail DTO includes assigned active tags** (`CoffeeShopDetailsDto.Tags`).
4. **Owner suggestion = out of scope** (later moderation flow).
5. **Seed starter tags in migration** `AddShopTags`: `laptop_friendly`, `specialty`, `pet_friendly`, `pour_over`, `quiet_work`.

## Non-goals (v1 of this feature)

- User-created tags
- Multi-language tag UI beyond `Name`
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

## API

### Admin (Authorization: Admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/admin/shop-tags` | List all tags (incl. inactive) |
| POST | `/api/admin/shop-tags` | Create tag |
| PATCH | `/api/admin/shop-tags/{id}` | Update name/description/sort/active (not slug) |
| DELETE | `/api/admin/shop-tags/{id}` | Soft-deactivate |
| PUT | `/api/admin/shops/{shopId}/tags` | Replace tag set on shop |

### Public / catalogs

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/Catalogs/shop-tags` | Active tags only (cached) |

### Search

`GET /api/CoffeeShops` query params:

```
tags: Guid[]        // AND semantics
isOpen: bool?       // computed from schedules at UTC query time
isNew: bool?        // CreatedAt within BusinessConstants.ItNewEntityInDays
isVisited: bool?    // only when UserId present; ignored for anonymous
```

## Caching

- Catalog: `CacheKey.Shop.TagsCatalog()` / pattern `shop:tags:*`
- Search cache hash includes `tags`, `isOpen`, `isNew`, `isVisited` (+ `userId` when visited filter used)
- Invalidate tags catalog + search (+ shop detail on SetShopTags) on admin writes

## Acceptance criteria

1. Admin CRUD tags; inactive tags hidden from public catalog.
2. Assigning tags to shop reflected in `GET /api/CoffeeShops?tags=...`.
3. Search with multiple tags uses AND.
4. `IsOpen` / `IsNew` / `IsVisited` work without Favorite.
5. Tests: domain SetTags max/distinct; admin handlers; search hash includes filters.
