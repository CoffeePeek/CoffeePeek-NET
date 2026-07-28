---
id: 260728-umo
slug: admin-coffee-shop-photo-reorder-add-sort
status: in-progress
---

# Quick: Admin coffee shop photo reorder

## Goal
Admins (and owners) can reorder published coffee shop gallery photos. Persist order via `SortIndex`. Document API for the frontend admin app.

## Tasks

### Wave 1 — Domain + schema
1. Add `SortIndex` to `ShopPhoto` + `SetSortIndex`
2. `CoffeeShop.AddPhotos` assigns contiguous indices; `ReorderPhotos(IReadOnlyList<Guid>)` validates full permutation
3. EF config + migration with backfill by `CreatedAtUtc`
4. Repository `GetById*` includes `ShopPhotos`

### Wave 2 — Contracts + reads
5. Additive `Id` + `SortIndex` on `ShortPhotoMetadataDto` / `PhotoMetadataDto`
6. Mapster: map Id/SortIndex; order photos by SortIndex
7. Extend `AdminPublishedShopDto` with ordered `Photos` + URL mapping via `MediaPublicUrlOptions`

### Wave 3 — APIs
8. `PUT /api/admin/shops/{id}/photos/order` `{ photoIds: Guid[] }`
9. `PUT /api/owner/coffee-shops/{id}/photos/order` (same body, ownership check)
10. Domain tests for reorder; frontend doc `docs/frontend-shop-photo-order.md`

## Out of scope
Upload/delete/replace photos; moderation draft gallery order.
