---
status: complete
id: 260728-umo
slug: admin-coffee-shop-photo-reorder-add-sort
---

# Summary: Admin coffee shop photo reorder

## Done
- `ShopPhoto.SortIndex` + EF migration with backfill by `CreatedAtUtc`
- `CoffeeShop.ReorderPhotos` + `AddPhotos` assigns indices
- Admin/Owner GET shop returns ordered `photos`
- `PUT /api/admin/shops/{id}/photos/order` and `PUT /api/owner/coffee-shops/{id}/photos/order`
- Additive `Id`/`SortIndex` on public photo DTOs; Mapster orders by SortIndex
- Frontend doc: `docs/frontend-shop-photo-order.md`
- Domain tests for reorder (7 CoffeeShopTests passed)

## Notes
- Apply shops migration on deploy: `make up-shops`
- List endpoints may return empty `photos`; use get-by-id for reorder UI
