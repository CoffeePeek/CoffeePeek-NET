---
status: complete
id: 260728-umo
slug: admin-coffee-shop-photo-reorder-add-sort
---

# Summary: Admin coffee shop photo reorder

## Done

- `ShopPhoto.SortIndex` + EF migration `AddShopPhotoSortIndex` (column, index, backfill by `CreatedAtUtc`) included in this PR
- `CoffeeShop.ReorderPhotos` returns FluentResults `Result`; handlers map failures to HTTP 400
- Admin/Owner GET shop returns ordered `photos`
- `PUT /api/admin/shops/{id}/photos/order` and `PUT /api/owner/coffee-shops/{id}/photos/order`
- Additive `Id`/`SortIndex` on public photo DTOs; Mapster orders by SortIndex
- Frontend doc: `docs/frontend-shop-photo-order.md`
- Domain unit tests for reorder (including duplicates, empty gallery, continued AddPhotos)

## Remaining (deploy / QA)

- Apply shops migration on target environments (`make up-shops`) before or with the new service image
- Manual end-to-end: admin/owner reorder flow against a deployed stack
