# Architecture Map
*Last mapped: 2026-05-17*

## Pattern

Microservices with layered DDD per bounded context. API Gateway at the edge, 4 downstream services, each with its own PostgreSQL database.

```
Client (Avalonia MVVM)
    ↓
API Gateway (YARP) — JWT validation, claims→headers, rate limiting, response caching
    ↓
┌──────────────┬──────────────┬──────────────┬──────────────┐
│   Account    │    Shops     │  Moderation  │    Media     │
│   Service    │   Service    │   Service    │   Service    │
└──────┬───────┴──────┬───────┴──────┬───────┴──────┬───────┘
       │              │              │              │
    PostgreSQL     PostgreSQL     PostgreSQL     PostgreSQL
                                              + MinIO (S3)
                      RabbitMQ (async messaging between services)
                      Redis (shared cache)
```

## Layers (per bounded context)

```
CoffeePeek.<Context>.Domain          — Aggregates, Entities, Value Objects, Domain Events
CoffeePeek.<Context>.Application     — CQRS handlers (Wolverine), Commands, Queries, DTOs
CoffeePeek.<Context>.Infrastructure  — Repository impls, external service adapters
CoffeePeek.<Context>.Persistence     — EF Core DbContext, migrations, query services
CoffeePeek.<Context>Service          — Service host: Program.cs, DependencyInjection.cs
```

MediaService is the exception — all layers reside in one project (`CoffeePeek.MediaService`).

## CQRS with Wolverine

Handlers use static methods with parameter injection:

```csharp
public class CreateShopHandler
{
    public static async Task<ShopCreatedResponse> Handle(
        CreateShopCommand cmd,
        IShopRepository repo,
        IUnitOfWork uow,
        CancellationToken ct) { ... }
}
```

Invoked via `IMessageBus.InvokeAsync<TResult>(command)`. Handlers discovered by assembly scanning in each service's `DependencyInjection.cs`.

Feature folders: `Application/Features/<Feature>/<UseCase>/` — command record and handler in the same folder.

## Gateway Authentication Flow

1. Gateway validates JWT (Bearer token)
2. Extracts claims → sets `X-User-Id` and `X-User-Roles` headers
3. Downstream services read these via `HeaderAuthenticationHandler` + `ClaimsPrincipalExtensions` from `CoffeePeek.Shared.Auth`
4. Downstream services do NOT re-validate JWT

Rate limits (per Gateway config):
- Global: 300 req/min
- Auth endpoints: 20 req/min
- Media endpoints: 10 req/min

## Cross-Service Messaging

RabbitMQ via Wolverine with transactional PostgreSQL outbox. Contracts defined in `CoffeePeek.Contract` (shared DTOs and event contracts).

## Caching

Redis via StackExchange.Redis. Typed `CacheKey` record for cache key construction. Graceful degradation on Redis failure. Decorator pattern for cache invalidation: `CachedUserRepository` wraps `UserRepository`.

## Persistence Pattern

```csharp
services.AddPersistence<AccountDbContext>(...)
    .AddRepository<IAccountRepository, AccountRepository>()
    .AddQueryService<IAccountQueryService, AccountQueryService>()
```

`CoffeePeek.Shared.Persistence` wires: EF Core + Npgsql, Wolverine PostgreSQL outbox, RabbitMQ auto-provisioning, Redis.

## Aspire Orchestration

`CoffePeek.AppHost` starts all services + PostgreSQL/Redis/RabbitMQ containers. Service discovery via Aspire names used in YARP cluster config.

## Shared Libraries

| Library | Purpose |
|---------|---------|
| `CoffeePeek.Shared.Kernel` | FluentResults, base abstractions |
| `CoffeePeek.Shared.Domain` | Domain primitives |
| `CoffeePeek.Shared.Persistence` | EF Core, Wolverine, PostgreSQL, RabbitMQ, Redis setup |
| `CoffeePeek.Shared.Auth` | JWT bearer, `HeaderAuthenticationHandler`, `ClaimsPrincipalExtensions` |
| `CoffeePeek.Shared.Web` | OpenAPI, API versioning, CORS, exception handling, Serilog/Sentry |
| `CoffeePeek.Contract` | Shared DTOs and event contracts |

## Client Architecture (Avalonia MVVM)

- ViewModels: `ViewModelBase`, `[ObservableProperty]`, `[RelayCommand]` (CommunityToolkit.Mvvm)
- Views: `x:DataType` compiled bindings
- View Locator: replaces `"ViewModel"` → `"View"` in type name
- DI: Autofac via `Bootstrapper.BuildContainer()` with modules
- Navigation: `INavigationService.NavigateTo<TViewModel>()`
- Platform entry points: Desktop, Browser, Android
