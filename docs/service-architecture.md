# Архитектура межсервисных взаимодействий CoffeePeek

## Обзор

CoffeePeek построен на микросервисной архитектуре с четкой изоляцией сервисов. Все межсервисные взаимодействия происходят через:
- **API Gateway** (YARP) - для синхронных HTTP запросов
- **События (MassTransit/RabbitMQ)** - для асинхронных взаимодействий

## Принципы изоляции

1. **Нет прямых зависимостей между сервисами** - все сервисы зависят только от общих проектов:
   - `CoffeePeek.Contract` - контракты (DTO, Events, Requests, Responses)
   - `CoffeePeek.Shared.*` - общие инфраструктурные компоненты
   - `CoffeePeek.Data` - общие репозитории

2. **Нет прямых HTTP вызовов между сервисами** - все запросы идут через Gateway

3. **Нет прямых обращений к БД других сервисов** - каждый сервис работает только со своей БД

4. **Асинхронные взаимодействия через события** - сервисы общаются через MassTransit события

## Сервисы

### 1. AuthService
**Назначение**: Аутентификация и авторизация пользователей

**Входные интерфейсы**:
- HTTP через Gateway: `/api/auth/*`
- События: нет входящих событий

**Исходящие события**:
- `UserLoggedInEvent` - публикуется при успешном логине
- `UserRegisteredEvent` - публикуется при регистрации

**Зависимости**:
- Собственная БД: `AuthDbContext`
- Redis: для кэширования credentials
- RabbitMQ: для публикации событий

### 2. UserService
**Назначение**: Управление профилями пользователей

**Входные интерфейсы**:
- HTTP через Gateway: `/api/user/*`
- События:
  - `UserRegisteredEvent` - создание профиля при регистрации
  - `UserLoggedInEvent` - обновление статистики при логине
  - `CheckinCreatedEvent` - обновление статистики при чек-ине
  - `ReviewAddedEvent` - обновление статистики при добавлении отзыва
  - `CoffeeShopApprovedEvent` - обновление статистики при одобрении магазина

**Исходящие события**: нет

**Зависимости**:
- Собственная БД: `UserDbContext`
- Redis: для кэширования профилей
- RabbitMQ: для подписки на события

### 3. ShopsService
**Назначение**: Управление кофейнями, отзывами, чек-инами

**Входные интерфейсы**:
- HTTP через Gateway:
  - `/api/shops/*` → `/api/CoffeeShop/*`
  - `/api/CheckIn/*`
  - `/api/internal/*`
- События:
  - `CoffeeShopApprovedEvent` - создание магазина после одобрения модерацией

**Исходящие события**:
- `CheckinCreatedEvent` - при создании чек-ина
- `ReviewAddedEvent` - при добавлении отзыва
- `CoffeeShopApprovedEvent` - при одобрении магазина (публикуется модерацией, потребляется здесь)

**Зависимости**:
- Собственная БД: `ShopsDbContext`
- Redis: для кэширования магазинов, городов, справочников
- RabbitMQ: для публикации и подписки на события

### 4. ModerationService
**Назначение**: Модерация кофеен

**Входные интерфейсы**:
- HTTP через Gateway: `/api/moderation/*`
- События: нет входящих событий

**Исходящие события**:
- `CoffeeShopApprovedEvent` - при одобрении магазина для модерации

**Зависимости**:
- Собственная БД: `ModerationDbContext`
- Внешний API: Yandex Geocoding (через HttpClient с resilience policies)
- RabbitMQ: для публикации событий

### 5. JobVacanciesService
**Назначение**: Управление вакансиями с hh.ru

**Входные интерфейсы**:
- HTTP через Gateway: `/api/vacancies/*` → `/api/vacancies/*`
- События: нет входящих событий

**Исходящие события**: нет

**Зависимости**:
- Собственная БД: `JobVacanciesDbContext`
- Внешний API: hh.ru API (через HttpClient с resilience policies)
- Hangfire: для фоновых задач синхронизации
- RabbitMQ: не используется

### 6. PhotoApi
**Назначение**: Управление фотографиями

**Входные интерфейсы**:
- HTTP через Gateway: `/api/photo/*` → `/api/*`
- События: нет

**Исходящие события**: нет

**Зависимости**:
- Собственное хранилище: файловая система/объектное хранилище
- RabbitMQ: не используется

### 7. Gateway
**Назначение**: Единая точка входа для всех HTTP запросов

**Функции**:
- Маршрутизация запросов к соответствующим сервисам (YARP Reverse Proxy)
- Агрегация Swagger документации
- Кэширование ответов
- CORS управление

**Маршруты Gateway**:
- `/api/auth/*` → AuthService
- `/api/user/*` → UserService
- `/api/shops/*` → ShopsService (трансформируется в `/api/CoffeeShop/*`)
- `/api/CheckIn/*` → ShopsService
- `/api/internal/*` → ShopsService
- `/api/moderation/*` → ModerationService
- `/api/photo/*` → PhotoApi (трансформируется в `/api/*`)
- `/api/vacancies/*` → JobVacanciesService (трансформируется в `/api/vacancies/*`)

## События (Event-Driven Communication)

### UserLoggedInEvent
- **Источник**: AuthService
- **Потребители**: UserService
- **Назначение**: Обновление статистики пользователя при логине

### UserRegisteredEvent
- **Источник**: AuthService
- **Потребители**: UserService
- **Назначение**: Создание профиля пользователя при регистрации

### CoffeeShopApprovedEvent
- **Источник**: ModerationService
- **Потребители**: ShopsService, UserService
- **Назначение**: 
  - ShopsService: создание магазина в основной БД
  - UserService: обновление статистики пользователя

### CheckinCreatedEvent
- **Источник**: ShopsService
- **Потребители**: UserService
- **Назначение**: Обновление статистики пользователя при чек-ине

### ReviewAddedEvent
- **Источник**: ShopsService
- **Потребители**: UserService
- **Назначение**: Обновление статистики пользователя при добавлении отзыва

## Общие компоненты

### CoffeePeek.Contract
Содержит все контракты между сервисами:
- DTOs
- Events
- Requests
- Responses
- Enums
- Constants

### CoffeePeek.Shared.Extensions
Общие расширения для всех сервисов:
- Модули конфигурации (Cache, Auth, Messaging, etc.)
- Middleware
- Swagger настройки
- Resilience policies (Polly)

### CoffeePeek.Shared.Infrastructure
Общая инфраструктура:
- Redis сервисы
- Кэш стратегии
- JWT настройки
- Health checks

### CoffeePeek.Data
Общие репозитории и интерфейсы:
- `IGenericRepository<T>`
- `IUnitOfWork`
- Реализации для EF Core

## Внешние зависимости

### Redis
Используется для:
- Кэширования данных (гибридный кэш: memory + distributed)
- Инвалидации кэша по тегам

### RabbitMQ
Используется для:
- Асинхронной коммуникации через события
- Outbox pattern для надежной доставки событий

### PostgreSQL
Каждый сервис имеет свою БД:
- `AuthDbContext` - AuthService
- `UserDbContext` - UserService
- `ShopsDbContext` - ShopsService
- `ModerationDbContext` - ModerationService
- `JobVacanciesDbContext` - JobVacanciesService

## Внешние API

### Yandex Geocoding API
- **Используется**: ModerationService
- **Назначение**: Геокодирование адресов
- **Реализация**: HttpClient с Circuit Breaker и Retry policies

### hh.ru API
- **Используется**: JobVacanciesService
- **Назначение**: Синхронизация вакансий
- **Реализация**: HttpClient с Circuit Breaker и Retry policies

## Правила разработки

1. **Новые сервисы** должны:
   - Зависеть только от `CoffeePeek.Contract` и `CoffeePeek.Shared.*`
   - Использовать Gateway для HTTP endpoints
   - Использовать события для асинхронных взаимодействий

2. **Новые события** должны:
   - Определяться в `CoffeePeek.Contract/Events`
   - Быть immutable records
   - Содержать только необходимые данные

3. **Новые HTTP endpoints** должны:
   - Регистрироваться в Gateway (`YarpRouteFactory`)
   - Использовать версионирование API (если требуется)
   - Документироваться в Swagger

4. **Кэширование**:
   - Использовать `IHybridCache` для cache-aside pattern
   - Инвалидировать кэш через `ICacheInvalidationStrategy` при изменениях
   - Использовать теги для групповой инвалидации

## Диаграмма взаимодействий

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ HTTP
       ▼
┌─────────────────┐
│  API Gateway     │
│   (YARP)        │
└──────┬───────────┘
       │
       ├───► AuthService ────┐
       ├───► UserService ◄───┤
       ├───► ShopsService ◄──┤
       ├───► ModerationService│
       ├───► JobVacanciesService
       └───► PhotoApi

       RabbitMQ Events:
       AuthService ──UserLoggedInEvent──► UserService
       AuthService ──UserRegisteredEvent──► UserService
       ModerationService ──CoffeeShopApprovedEvent──► ShopsService, UserService
       ShopsService ──CheckinCreatedEvent──► UserService
       ShopsService ──ReviewAddedEvent──► UserService
```

## Проверка изоляции

Для проверки изоляции сервисов выполните:

1. Проверка зависимостей в csproj:
   ```bash
   # Убедитесь, что нет прямых ProjectReference между сервисами
   grep -r "ProjectReference.*Service" *.csproj
   ```

2. Проверка прямых HTTP вызовов:
   ```bash
   # Убедитесь, что нет прямых вызовов к другим сервисам
   grep -r "http://.*service\|localhost.*service" --include="*.cs"
   ```

3. Проверка прямых обращений к БД:
   ```bash
   # Убедитесь, что каждый сервис использует только свой DbContext
   grep -r "DbContext" --include="*.cs" | grep -v "Test"
   ```

## Обновление документации

При добавлении новых сервисов или изменении взаимодействий обновите этот документ.

