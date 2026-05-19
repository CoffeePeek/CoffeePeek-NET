# External Integrations

**Analysis Date:** 2026-05-17

## APIs & External Services

**Authentication (OAuth):**
- Google Sign-In — validates Google ID tokens for OAuth login
  - SDK/Client: `Google.Apis.Auth` 1.73.0
  - Implementation: `CoffeePeek.Account.Infrastructure/Identity/GoogleAuthService.cs`
  - Auth: `OAuthGoogleOptions.ClientId` / `OAuthGoogleOptions.ClientSecret` (config section `OAuthGoogleOptions`)
  - Token validation via `GoogleJsonWebSignature.ValidateAsync`

**Email (Transactional):**
- Resend — sends email confirmation and welcome-back emails
  - SDK/Client: `Resend` 0.3.0
  - Implementation: `CoffeePeek.Account.Infrastructure/Consumers/UserRegisteredEventHandler.cs`
  - Auth: `ResendClientOptions.ApiToken` (config section `ResendClientOptions`)
  - Sender address: `CoffeePeek.by <info@coffeepeek.by>`
  - Registered in DI: `CoffeePeek.Account.Infrastructure/DependencyInjection.cs` via `services.AddTransient<IResend, ResendClient>()`

**Geocoding:**
- Yandex Geocoding API — converts addresses to coordinates during shop moderation/approval
  - SDK/Client: `HttpClient` (no dedicated SDK, plain HTTP)
  - Implementation: `CoffeePeek.Moderation.Infrastructure/Services/YandexGeocodingService.cs`
  - Auth: `YandexApiOptions.ApiKey` (config section `YandexApiOptions`)
  - Base URL: `https://geocode-maps.yandex.ru/1.x/` (configurable via `YandexApiOptions.BaseUrl`)
  - Timeout: `YandexApiOptions.TimeoutSeconds` (default 30)

## Data Storage

**Databases:**
- PostgreSQL 17 — primary data store for all four services; four isolated databases
  - `account` DB — user accounts, refresh tokens, email confirmations; `AccountDbContext` in `CoffeePeek.Account.Persistence/Configuration/`
  - `shops` DB — coffee shops, reviews, check-ins, favorites; `ShopsDbContext` in `CoffeePeek.Shops.Persistance/Configuration/`
  - `moderation` DB — shop suggestions, moderation reviews; `ModerationDbContext` in `CoffeeShop.Moderation.Persistence/Configuration/`
  - `media` DB — photo metadata and upload state; `MediaDbContext` in `CoffeePeek.MediaService/Data/`
  - Connection: `PostgresCpOptions.ConnectionString` per service
  - Client: EF Core 10 + Npgsql 10.0.1; `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` for Aspire-managed connection strings
  - Migrations: EF Core migrations committed per persistence project under `Migrations/` subdirectories

**Message Broker:**
- RabbitMQ — asynchronous event bus between services (domain events for user registration, check-ins, photo uploads, moderation decisions)
  - SDK/Client: `WolverineFx.RabbitMQ` 5.30.0
  - Configuration: `RabbitMqOptions` (`HostName`, `Username`, `Password`, `Port`, `VirtualHost`)
  - Provisioning: `AutoProvision()` called in `CoffeePeek.Shared.Persistence/Extensions/WolverineModule.cs` — queues and exchanges created automatically
  - Outbox pattern: `WolverineFx.Postgresql` persists messages in PostgreSQL before RabbitMQ delivery

**File Storage:**
- MinIO (S3-compatible) — stores user avatars and coffee shop photos
  - SDK/Client: `Minio` 7.0.0
  - Implementation: `CoffeePeek.MediaService/Services/MinIOStorageService.cs`
  - Auth: `MinIOOptions.AccessKey` / `MinIOOptions.SecretKey`
  - Endpoint: `MinIOOptions.Endpoint`
  - Buckets: `coffee.avatars` (user avatars, `MinIOOptions.UserBucketName`), `coffee.shops` (shop photos, `MinIOOptions.ShopBucketName`)
  - Public bucket URL hardcoded in Mapster configurations: `https://bucket-dev-771f.up.railway.app/` — needs environment-aware configuration (see CONCERNS.md)

**Caching:**
- Redis — caching and session-adjacent invalidation
  - SDK/Client: `StackExchange.Redis` 2.12.14
  - Configuration: `RedisOptions` (`Host`, `Port`, `Password`)
  - Implementation: `CoffeePeek.Shared.Persistence/Cache/Redis/RedisService.cs`
  - DI: singleton `IConnectionMultiplexer` registered in `CoffeePeek.Shared.Persistence/Extensions/RedisConfiguration.cs`

## Authentication & Identity

**Auth Provider:**
- Custom JWT — tokens issued by AccountService, validated at Gateway
  - JWT signing: `JWTOptions.SecretKey` (minimum 32 chars, validated on startup)
  - Token structure: `Issuer = "CoffeePeek.WEB"`, `Audience = "CoffeePeek.API"`
  - Access token lifetime: `JWTOptions.AccessTokenLifetimeMinutes` (1–1440)
  - Refresh token lifetime: `JWTOptions.RefreshTokenLifetimeDays` (1–365)
  - Implementation: `CoffeePeek.Account.Infrastructure/Identity/JWTTokenService.cs`
  - Gateway validation: `CoffeePeek.Shared.Auth/Extensions/AuthModule.cs`, JWT bearer middleware
  - Downstream auth: claims forwarded via HTTP headers; `CoffeePeek.Shared.Auth/HeaderAuthenticationHandler.cs` reconstructs `ClaimsPrincipal` from headers
  - Role constants: `CoffeePeek.Shared.Auth/Constants/RoleConsts.cs`

**Google OAuth:**
- ID token validation only — Google OAuth sign-in flow managed by the Avalonia client; server verifies the ID token
  - Implementation: `CoffeePeek.Account.Infrastructure/Identity/GoogleAuthService.cs`
  - No server-side OAuth code exchange; client sends `idToken` to `OAuthLoginCommand`

## Monitoring & Observability

**Error Tracking:**
- Sentry — error and exception tracking for Account, Shops, Moderation services
  - SDK: `Sentry.AspNetCore` 6.3.1
  - Configuration: `Sentry.Dsn` in `appsettings.json` per service (empty by default, must be injected)
  - Config keys: `SendDefaultPii`, `MaxRequestBodySize`, `MinimumBreadcrumbLevel`, `MinimumEventLevel`, `AttachStackTrace`
  - Registered via `CoffeePeek.Shared.Web` (included in `WebApplicationBuilder` setup)

**Distributed Tracing / Metrics:**
- OpenTelemetry — ASP.NET Core, HTTP client, and runtime instrumentation
  - Exporter: OTLP (`OTEL_EXPORTER_OTLP_ENDPOINT` env var); if unset, no remote export
  - Instruments: `AspNetCore`, `HttpClient`, `Runtime` metrics; `AspNetCore` + `HttpClient` traces
  - Health check endpoints excluded from tracing
  - Configuration: `CoffePeek.ServiceDefaults/Extensions.cs`

**Logs:**
- Serilog — structured console logging on all services
  - Output: console only (no file or remote sink configured)
  - Configuration: `CoffeePeek.Shared.Web/Logging/SerilogExtensions.cs`; reads from `appsettings.json` via `ReadFrom.Configuration`
  - Format: `[HH:mm:ss LVL] SourceContext Message\nException`

**Health Checks:**
- ASP.NET Core Health Checks — `/health` and `/alive` endpoints (development only via `MapDefaultEndpoints`)
  - "self" liveness check registered for all services

## CI/CD & Deployment

**Hosting:**
- Railway — production platform
  - Internal service communication via `*.railway.internal` hostnames (configured in `CoffeePeek.Gateway/appsettings.json`)
  - MinIO bucket hosted at `https://bucket-dev-771f.up.railway.app/` (dev URL hardcoded in Mapster configs)

**Container Registry:**
- Docker Hub — images pushed on `main` / `dev` branch push
  - Auth: `DOCKER_HUB_USERNAME` / `DOCKER_HUB_PASSWORD` GitHub secrets
  - Images: `coffeepeek.accountservice`, `coffeepeek.shopsservice`, `coffeepeek.moderationservice`, `coffeepeek.mediaservice`, `gateway`

**CI Pipeline:**
- GitHub Actions — `.github/workflows/ci-cd.yml`
  - Triggers: push or PR to `dev` or `main`
  - Jobs: `build_and_test` → `build_and_push_images` → `apply_migrations`
  - Runtime: Ubuntu Latest, .NET 10, Java 17 (Temurin, required for Android workload)
  - NuGet cache keyed on `**/*.csproj`, `Directory.Packages.props`, `global.json`
  - Vulnerability audit: `dotnet list package --vulnerable` (non-blocking, summary posted to job)
  - Migration secrets: `DB_CONNECTION_ACCOUNT`, `DB_CONNECTION_SHOPS`, `DB_CONNECTION_MODERATION`, `DB_CONNECTION_MEDIA`

## Environment Configuration

**Required env vars / config secrets:**
- `JWTOptions__SecretKey` — JWT signing key (min 32 chars)
- `PostgresCpOptions__ConnectionString` — per-service PostgreSQL connection string
- `RabbitMqOptions__HostName`, `__Username`, `__Password`, `__Port`, `__VirtualHost` — RabbitMQ broker
- `RedisOptions__Host`, `__Port`, `__Password` — Redis
- `MinIOOptions__Endpoint`, `__AccessKey`, `__SecretKey`, `__UserBucketName`, `__ShopBucketName` — object storage
- `ResendClientOptions__ApiToken` — Resend email API key
- `OAuthGoogleOptions__ClientId`, `__ClientSecret` — Google OAuth
- `YandexApiOptions__ApiKey` — Yandex Geocoding
- `Sentry__Dsn` — Sentry DSN per service
- `WebClientUrl` — frontend origin for email confirmation links
- `ALLOWED_ORIGINS` — comma-separated list of CORS-allowed origins (production only)
- `OTEL_EXPORTER_OTLP_ENDPOINT` — OTLP collector endpoint (optional)

**Secrets location:**
- Development: .NET User Secrets (`UserSecretsId` in `CoffePeek.AppHost`)
- Production: GitHub Actions secrets + Railway environment variables

## Webhooks & Callbacks

**Incoming:**
- None — no inbound webhook endpoints detected

**Outgoing:**
- Resend email send (`EmailSendAsync`) — called when `UserRegisteredInternalEvent` is handled
- Yandex Geocoding HTTP GET — called during shop moderation approval to resolve coordinates

---

*Integration audit: 2026-05-17*
