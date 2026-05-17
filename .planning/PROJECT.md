# CoffeePeek — Tech Debt Resolution

## What This Is

CoffeePeek — это .NET 10 микросервисная платформа для поиска и открытия кофеен. Данный проект покрывает полное устранение технического долга, задокументированного в `.planning/codebase/CONCERNS.md`: от критических уязвимостей безопасности и функциональных багов — до рефакторинга кода, оптимизации производительности и восстановления тестового покрытия.

## Core Value

Каждый баг, уязвимость и проблема производительности, найденные в CONCERNS.md, должны быть исправлены и закрыты тестом — так, чтобы они больше не могли вернуться.

## Requirements

### Validated

- ✓ Microservices architecture (Account, Shops, Moderation, Media, Gateway) — существует
- ✓ JWT auth via YARP Gateway с X-User-Id headers — существует
- ✓ CQRS via Wolverine static handlers — существует
- ✓ EF Core + PostgreSQL persistence — существует
- ✓ Redis caching с CacheKey abstractions — существует
- ✓ RabbitMQ messaging с Wolverine outbox — существует

### Active

- [ ] Устранение технического долга (#if DEBUG branching, CancellationToken.None, пустой репозиторий, дублирующий вызов UpdateDetails, InvalidOperationException вместо DomainException)
- [ ] Исправление критических багов (cache key mismatch, SearchCoffeeShops generic error, ownership check на Delete Review, UserId игнорируется в GetCoffeeShop, CoffeeShopApprovedEventHandler без SaveChangesAsync)
- [ ] Устранение security уязвимостей (Sentry PII, rate limiting по IP, Yandex API key в URL, YARP health checks)
- [ ] Исправление проблем производительности (correlated subquery MinRating, LIKE с leading wildcard, Redis KEYS blocking, Include + ProjectTo overhead, UserNameChangedHandler N+1)
- [ ] Написание тестов для Shops Application, Shops Domain, критических путей

### Out of Scope

- Именованные опечатки в проектах (CoffePeek, Persistance, CoffeeShop.Moderation.*) — приняты как-есть согласно CLAUDE.md, переименование сломает историю git без практической пользы
- Новые функциональные фичи — только фиксы
- Moderation/Media тесты — низкий приоритет для данного milestone (Shops и Account критичнее)
- PostGIS/кластеризация карты — отдельный milestone по гео-функциональности

## Context

**Codebase state:** `.planning/codebase/` — полная карта архитектуры, стека, соглашений и проблем.

**Критичность находок:**
- `DeleteReviewFromCoffeeShop` — любой авторизованный пользователь может удалить чужой отзыв (CRITICAL)
- `SendDefaultPii: true` — в случае заполнения Sentry DSN, пароли пользователей уходят в Sentry (HIGH)
- Cache key mismatch для CoffeeBeans — кэш никогда не инвалидируется (HIGH)
- Rate limiting по raw IP — бесполезен за load balancer (HIGH)
- `GetCoffeeShopQuery` игнорирует UserId — IsFavorite/IsVisited всегда false для авторизованных (MEDIUM)

**Сервис в фокусе:** `CoffeePeek.Shops.*` содержит большинство критических проблем.

**Тестирование:** xUnit v3 + FluentAssertions + Moq. Нулевое покрытие Shops Application handlers.

## Constraints

- **Tech Stack**: .NET 10, C# 14 — не менять
- **Breaking changes**: Нельзя менять публичные API контракты (CoffeePeek.Contract) без версионирования
- **Naming typos**: Не переименовывать CoffePeek.*, Shops.Persistance, CoffeeShop.Moderation.* — принято решение CLAUDE.md
- **Migration safety**: Изменения схемы БД (если потребуются) через EF Core migrations с Makefile

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Порядок фаз: Tech Debt → Bugs → Security → Perf → Tests | Начать с чистоты кода, затем корректность, потом безопасность | — Pending |
| Тесты — отдельная фаза (Phase 5) | Написаны после исправлений, чтобы фиксировать правильное поведение | — Pending |
| #if DEBUG → runtime feature flag | `IHostEnvironment` или appsettings вместо compile-time branching | — Pending |
| Sentry PII → false по умолчанию | Безопасное значение в committed config, override только в local dev | — Pending |

## Evolution

После каждой фазы:
1. Исправленные требования → перенести в Validated с номером фазы
2. Новые проблемы → добавить в Active
3. Решения → зафиксировать в Key Decisions

---
*Last updated: 2026-05-17 после инициализации проекта*
