---
phase: 05-test-coverage
plan: "05"
subsystem: shops-application-tests
tags:
  - testing
  - unit-tests
  - cache
  - shops
dependency_graph:
  requires:
    - 05-01
    - 05-02
    - 05-03
  provides:
    - SearchCoffeeShopsHandler test coverage
    - GetCoffeeShopHandler test coverage
    - TEST-01 fully closed
  affects:
    - CoffeePeek.Shops.Application.Tests
tech_stack:
  added: []
  patterns:
    - ICacheService factory mock with 4-parameter overload (Pitfall 1)
    - Moq ReturnsAsync bypassing async factory
    - IMapper mock not configured when factory is bypassed
key_files:
  created:
    - CoffeePeek.Shops.Application.Tests/Features/CoffeeShop/SearchCoffeeShopsHandlerTests.cs
    - CoffeePeek.Shops.Application.Tests/Features/CoffeeShop/GetCoffeeShopHandlerTests.cs
  modified: []
decisions:
  - "Used GetCoffeeShopResponse.ShopDto (not .Shop) — property name verified from source"
  - "IMapper mock not configured since cache factory is bypassed by ReturnsAsync"
  - "List<Guid> used for ReturnsAsync since GetFavoriteShopIdsAsync and GetVisitedShopIdsAsync return Task<List<Guid>>"
metrics:
  duration: "~10 minutes"
  completed: "2026-05-20"
  tasks_completed: 2
  files_created: 2
  tests_added: 6
---

# Phase 05 Plan 05: Cache-Heavy Handler Tests Summary

Добавлены тесты для двух сложнейших хэндлеров Shops Application, использующих `ICacheService` с async factory — `SearchCoffeeShopsHandler` и `GetCoffeeShopHandler`. TEST-01 полностью закрыт.

## What Was Built

**SearchCoffeeShopsHandlerTests.cs** — 3 теста:
1. `Handle_AnonymousRequest_ReturnsCachedResponse` — кэш возвращает данные, favoriteRepo и visitRepo не вызываются
2. `Handle_AuthenticatedRequest_EnrichesWithFavoriteAndVisitedFlags` — IsFavorite=true для shopId в избранном, IsVisited=false для shopId вне посещённых
3. `Handle_WhenCacheReturnsNull_ReturnsError` — IsSuccess=false, Message contains "Failed to retrieve"

**GetCoffeeShopHandlerTests.cs** — 3 теста:
1. `Handle_AnonymousRequest_ReturnsShopWithoutPersonalization` — ShopDto.Id верен, Exists не вызывается
2. `Handle_AuthenticatedRequest_EnrichesWithFavoriteAndVisited` — IsFavorite=true, IsVisited=false
3. `Handle_WhenShopNotFound_ReturnsError` — IsSuccess=false, Message contains "Shop not found"

## Test Results

```
Пройден! : не пройдено 0, пройдено 11, пропущено 0, всего 11
```

Полный прогон `dotnet test CoffeePeek.slnx`:
- CoffeePeek.Shops.Application.Tests: 11 passed (5 existing + 6 new)
- CoffeePeek.Shops.Domain.Tests: 12 passed
- CoffeePeek.Account.Domain.Tests: 207 passed
- CoffeePeek.Account.Application.Tests: 141 passed
- CoffeePeek.Account.Infrastructure.Tests: 35 passed
- **Total: 0 failed**

## Key Technical Decisions

### ICacheService Factory Mock (Pitfall 1)
Оба хэндлера вызывают `GetAsync(cacheKey, factory, TimeSpan?, ct)`. Решение:
```csharp
_cacheMock
    .Setup(c => c.GetAsync(
        It.IsAny<CacheKey>(),
        It.IsAny<Func<CancellationToken, Task<T>>>(),
        It.IsAny<TimeSpan?>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(expectedValue);
```
`ReturnsAsync` возвращает готовый объект без вызова factory. Это обходит необходимость настраивать зависимости внутри factory (ICoffeeShopQueries, IQueryReviewRepository, IMapper).

### GetCoffeeShopResponse.ShopDto
Имя свойства в ответе — `ShopDto`, не `Shop`. Верифицировано по исходному коду `GetCoffeeShopResponse.cs`:
```csharp
public sealed class GetCoffeeShopResponse(CoffeeShopDetailsDto shopDto)
{
    public CoffeeShopDetailsDto ShopDto { get; set; } = shopDto;
}
```

### IMapper не требует настройки
В `GetCoffeeShopHandler` mapper используется только внутри factory-лямбды (`mapper.Map<ReviewDto[]>(reviews)`). Поскольку мок возвращает `shopDto` напрямую через `ReturnsAsync`, factory никогда не вызывается.

## Commits

| Task | Description | Commit |
|------|-------------|--------|
| 1    | SearchCoffeeShopsHandlerTests (3 tests) | 86614dc |
| 2    | GetCoffeeShopHandlerTests (3 tests) | f224596 |

## Deviations from Plan

None — план выполнен точно. Единственное уточнение: в плане было написано `result.Data.Shop`, но в коде свойство называется `ShopDto`. Это замечание из плана ("читать GetCoffeeShopResponse.cs если нужно") было выполнено — использован правильный `result.Data.ShopDto`.

## Known Stubs

None.

## Threat Flags

None — тестовые файлы не вводят новых network endpoints, auth paths или schema changes.

## Self-Check: PASSED

- [x] `CoffeePeek.Shops.Application.Tests/Features/CoffeeShop/SearchCoffeeShopsHandlerTests.cs` — FOUND
- [x] `CoffeePeek.Shops.Application.Tests/Features/CoffeeShop/GetCoffeeShopHandlerTests.cs` — FOUND
- [x] Commit 86614dc — FOUND
- [x] Commit f224596 — FOUND
- [x] `dotnet test CoffeePeek.slnx` → 0 failed — VERIFIED
