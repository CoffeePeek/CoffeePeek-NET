# Requirements: CoffeePeek Tech Debt Resolution

**Defined:** 2026-05-17
**Core Value:** Каждый баг, уязвимость и проблема производительности из CONCERNS.md исправлены и закрыты тестом.

## v1 Requirements

### Tech Debt (Phase 1)

- [x] **TD-01**: `#if DEBUG` в persistence DI заменён на runtime feature flag через `IHostEnvironment` или appsettings во всех 4 сервисах
- [x] **TD-02**: Дублирующий вызов `UpdateDetails` после конструктора удалён из `CreateShopFromModerationService`
- [x] **TD-03**: `CancellationToken.None` заменён на проброс токена от caller в `AuthService` и `UserNameChangedEventHandler`
- [x] **TD-04**: `InvalidOperationException` в `UserFavoriteService` заменён на `ValidationException` для 400-ответа вместо 500
- [x] **TD-05**: `RevokeAllSessions()` удалён из login flow; `AddSession` управляет лимитом сессий самостоятельно
- [x] **TD-06**: Пустой `CoffeeShopRepository.cs` удалён или реализован
- [x] **TD-07**: `#if DEBUG` в shared libraries (`WolverineModule`, `CorsModule`, `GlobalExceptionHandler`) заменён на runtime config

### Known Bugs (Phase 2)

- [x] **BUG-01**: Cache key mismatch для CoffeeBeans исправлен — `CacheKey.CoffeeBean.ListPattern()` возвращает `"coffeebean:list:*"` вместо `"bean:list:*"`
- [x] **BUG-02**: `SearchCoffeeShopsHandler` возвращает описательный error message вместо пустого `"Error"` на null из Redis
- [ ] **BUG-03**: `DeleteReviewFromCoffeeShopHandler` проверяет `review.UserId == requestingUserId` перед удалением; не-владелец получает 403
- [x] **BUG-04**: `CoffeeShopsController.GetCoffeeShop` передаёт `userId` в `GetCoffeeShopQuery`; `IsFavorite`/`IsVisited` корректны для авторизованных пользователей
- [ ] **BUG-05**: `CoffeeShopApprovedEventHandler` явно вызывает `unitOfWork.SaveChangesAsync()` после `shopRepository.Add()`

### Security (Phase 3)

- [x] **SEC-01**: Sentry конфиг в committed `appsettings.json` имеет `SendDefaultPii: false` и `MaxRequestBodySize: "None"` во всех сервисах
- [x] **SEC-02**: YARP active health checks включены на всех 4 кластерах с `ConsecutiveFailures` политикой
- [x] **SEC-03**: Rate limiting в Gateway партиционируется по `X-Forwarded-For`/`X-Real-IP` вместо `RemoteIpAddress`
- [x] **SEC-04**: Yandex API ключ передаётся через заголовок запроса, а не встраивается в URL
- [x] **SEC-05**: `MapController` имеет явную политику rate limiting; отсутствие `[Authorize]` задокументировано в OpenAPI

### Performance (Phase 4)

- [ ] **PERF-01**: `MinRating` фильтр в `CoffeeShopQueries.Search` не использует correlated subquery — заменён на JOIN к pre-aggregated CTE или денормализованному полю `AverageRating`
- [ ] **PERF-02**: Полнотекстовый поиск по `Name`/`Address` использует `ILIKE` с trigram индексом (`pg_trgm`) или `tsvector` вместо `LOWER().Contains()`
- [ ] **PERF-03**: Лишний `.Include(x => x.CoffeeShop)` удалён из `GetUserFavoriteCoffeeShops` — `ProjectToType<>` работает без него
- [ ] **PERF-04**: `RedisService.RemoveByPattern` использует `SCAN` (через `pageSize` параметр) вместо блокирующего `KEYS`
- [ ] **PERF-05**: `UserNameChangedHandler` обновляет отзывы одним `ExecuteUpdateAsync` вместо загрузки всех в память

### Test Coverage (Phase 5)

- [ ] **TEST-01**: Unit тесты для всех Shops Application handlers: `SearchCoffeeShopsHandler`, `GetCoffeeShopHandler`, `CreateCheckInHandler`, `AddToFavoriteHandler`, `DeleteReviewFromCoffeeShopHandler`, `GetShopsInBoundsHandler`
- [ ] **TEST-02**: Unit тесты для Shops Domain: `CoffeeShop` aggregate, `Review.Create` validation, `CheckIn.Create` invariants
- [ ] **TEST-03**: Тест подтверждает, что `DeleteReviewFromCoffeeShop` возвращает 403 при несоответствии владельца
- [ ] **TEST-04**: Тест `CreateCheckInHandler` покрывает path с невалидным rating (DomainException не должен молча проглатываться)
- [ ] **TEST-05**: Unit тест для `Account.Application.Features.User.UpdateUserProfile.UpdateEmail` handler

## v2 Requirements

### Расширенные тесты

- **TEST-V2-01**: Integration tests с TestContainers для Shops persistence queries
- **TEST-V2-02**: Тесты Moderation Application handlers
- **TEST-V2-03**: Тесты Media service (GenerateAvatarUploadHandler, ConfirmPhotoHandler)

### Масштабирование

- **SCALE-01**: PostGIS `ST_ClusterDBSCAN` для viewport-кластеризации вместо hard limit 500 shops
- **SCALE-02**: Service-namespace prefix для Redis cache keys (`account:user:*`, `shops:shop:*`)

## Out of Scope

| Feature | Reason |
|---------|--------|
| Переименование CoffePeek.*, Shops.Persistance, CoffeeShop.* | Принято как-есть в CLAUDE.md; переименование сломает историю git |
| Новые функциональные фичи | Только фиксы |
| Media service тесты (v1) | Ниже по приоритету чем Shops/Account |
| Moderation service тесты (v1) | Ниже по приоритету |
| Wolverine PostgreSQL decoupling | Низкая срочность; документировать в ARCHITECTURE.md достаточно |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| TD-01 | Phase 1 | Complete |
| TD-02 | Phase 1 | Complete |
| TD-03 | Phase 1 | Complete |
| TD-04 | Phase 1 | Complete |
| TD-05 | Phase 1 | Complete |
| TD-06 | Phase 1 | Complete |
| TD-07 | Phase 1 | Pending |
| BUG-01 | Phase 2 | Complete |
| BUG-02 | Phase 2 | Complete |
| BUG-03 | Phase 2 | Pending |
| BUG-04 | Phase 2 | Complete |
| BUG-05 | Phase 2 | Pending |
| SEC-01 | Phase 3 | Complete |
| SEC-02 | Phase 3 | Complete |
| SEC-03 | Phase 3 | Complete |
| SEC-04 | Phase 3 | Complete |
| SEC-05 | Phase 3 | Complete |
| PERF-01 | Phase 4 | Pending |
| PERF-02 | Phase 4 | Pending |
| PERF-03 | Phase 4 | Pending |
| PERF-04 | Phase 4 | Pending |
| PERF-05 | Phase 4 | Pending |
| TEST-01 | Phase 5 | Pending |
| TEST-02 | Phase 5 | Pending |
| TEST-03 | Phase 5 | Pending |
| TEST-04 | Phase 5 | Pending |
| TEST-05 | Phase 5 | Pending |

**Coverage:**
- v1 requirements: 27 total
- Mapped to phases: 27
- Unmapped: 0 ✓

---
*Requirements defined: 2026-05-17*
*Last updated: 2026-05-17 после инициализации проекта*
