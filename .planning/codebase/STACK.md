# Technology Stack

**Analysis Date:** 2026-05-17

## Languages

**Primary:**
- C# 14 — all backend services, shared libraries, Avalonia client
- XML/XAML — Avalonia UI layouts

**Secondary:**
- SQL — EF Core migrations (PostgreSQL dialect)

## Runtime

**Environment:**
- .NET 10 (SDK 10.0.100, `rollForward: latestMajor`, allows pre-release)
- Pinned via `global.json`

**Package Manager:**
- NuGet with Central Package Management (`Directory.Packages.props`, `ManagePackageVersionsCentrally=true`)
- All version numbers declared once in `Directory.Packages.props` — individual `.csproj` files reference packages without version attributes

## Frameworks

**Core (Backend):**
- ASP.NET Core 10 (`Microsoft.NET.Sdk.Web`) — all five services (Account, Shops, Moderation, Media, Gateway)
- YARP 2.3.0 — reverse proxy at `CoffeePeek.Gateway`
- Wolverine 5.30.0 — CQRS message bus and saga orchestration (WolverineFx, WolverineFx.EntityFrameworkCore, WolverineFx.Postgresql, WolverineFx.RabbitMQ)
- EF Core 10.0.5 — ORM across all services; `Z.EntityFramework.Extensions.EFCore` 10.105.4 for bulk operations

**API Layer:**
- `Asp.Versioning.Http` 8.1.1 + `Asp.Versioning.Mvc.ApiExplorer` 8.1.1 — API versioning on downstream services
- `Microsoft.AspNetCore.OpenApi` 10.0.5 — OpenAPI schema generation
- `Scalar.AspNetCore` 2.13.22 — OpenAPI UI served at the Gateway (`/scalar`)

**Security:**
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.5 — JWT validation at the Gateway (`CoffeePeek.Shared.Auth`)
- Rate limiting — built-in ASP.NET Core sliding window, configured in `CoffeePeek.Gateway/Extensions/RateLimitingExtensions.cs`

**Client (Avalonia — multi-platform):**
- Avalonia 12 — cross-platform XAML UI (Desktop, Browser, Android)
- CommunityToolkit.Mvvm — `[ObservableProperty]` / `[RelayCommand]` source generators
- `Xaml.Behaviors.Avalonia` 12.0.0 — XAML behavior support
- Autofac 9.1.0 — DI container for the Avalonia client

**Testing:**
- xUnit v3 3.2.2 — test runner
- FluentAssertions 8.9.0 — assertion library
- Moq 4.20.72 — mocking framework
- coverlet.collector 8.0.1 — code coverage collection
- Bogus 35.6.5 — fake data generation (used in persistence test fixtures)

**Build/Dev:**
- .NET Aspire 13.2 — local dev orchestration (`CoffePeek.AppHost`)
- `CoffePeek.ServiceDefaults` — shared OpenTelemetry, service discovery, health checks for all services

## Key Dependencies

**Critical:**
- `WolverineFx` 5.30.0 — CQRS bus; all handler dispatch, RabbitMQ publish/subscribe, PostgreSQL outbox pattern (`CoffeePeek.Shared.Persistence`)
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.1 — database driver; used in all four data DbContexts (Account, Shops, Moderation, Media)
- `StackExchange.Redis` 2.12.14 — Redis client; session storage and cache invalidation (`CoffeePeek.Shared.Persistence/Extensions/RedisConfiguration.cs`)
- `Minio` 7.0.0 — MinIO/S3 object storage client (`CoffeePeek.MediaService`)
- `Google.Apis.Auth` 1.73.0 — Google ID token validation (`CoffeePeek.Account.Application`)
- `Resend` 0.3.0 — transactional email SDK (`CoffeePeek.Account.Application`, `CoffeePeek.Account.Infrastructure`)
- `Sentry.AspNetCore` 6.3.1 — error tracking (`CoffeePeek.Shared.Web`)
- `Serilog.AspNetCore` 10.0.0 + `Serilog.Sinks.Console` 6.1.1 — structured logging (`CoffeePeek.Shared.Web`)
- `FluentResults` 4.0.0 — Result<T> error handling pattern (`CoffeePeek.Shared.Kernel`)
- `Mapster` 10.0.7 + `Mapster.DependencyInjection` 10.0.7 — object mapping (Account Application, Shops Application, Moderation Application)
- `Polly` 8.5.2 + `Microsoft.Extensions.Http.Polly` 10.0.0 — HTTP resilience
- `Microsoft.Extensions.Http.Resilience` 10.4.0 — Aspire standard resilience handler

**Infrastructure:**
- `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` 13.2.2 — Aspire integration for PostgreSQL + EF Core in Account, Shops, Media persistence projects
- `Aspire.Hosting.AppHost` 13.2.2 — Aspire AppHost SDK
- `Aspire.Hosting.PostgreSQL` 13.2.2, `Aspire.Hosting.RabbitMQ` 13.2.2, `Aspire.Hosting.Redis` 13.2.2 — container orchestration in `CoffePeek.AppHost`
- `Microsoft.Extensions.ServiceDiscovery` 10.4.0 + `Microsoft.Extensions.ServiceDiscovery.Yarp` 10.4.0 — Aspire service discovery
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.15.3, `OpenTelemetry.Extensions.Hosting` 1.15.3, `OpenTelemetry.Instrumentation.AspNetCore` 1.15.2, `OpenTelemetry.Instrumentation.Http` 1.15.1, `OpenTelemetry.Instrumentation.Runtime` 1.15.1 — observability pipeline (`CoffePeek.ServiceDefaults`)
- `Microsoft.EntityFrameworkCore.Relational` 10.0.5 — relational mapping in Account and Shops persistence
- `System.IO.Hashing` 10.0.5 — used in Account Application

## Configuration

**Environment:**
- Configuration sourced from `appsettings.json`, environment variables, and .NET User Secrets (`UserSecretsId` in AppHost)
- Options pattern throughout: all config sections are strongly typed with `AddValidateOptions<T>()` (DataAnnotations validation on startup)
- Key configuration sections (see `CoffeePeek.AccountService/appsettings.json`):
  - `JWTOptions` — `SecretKey`, `Issuer`, `Audience`, `AccessTokenLifetimeMinutes`, `RefreshTokenLifetimeDays`
  - `PostgresCpOptions` — `ConnectionString`
  - `RabbitMqOptions` — `HostName`, `Username`, `Password`, `Port`, `VirtualHost`
  - `RedisOptions` — `Host`, `Port`, `Password`
  - `MinIOOptions` — `Endpoint`, `AccessKey`, `SecretKey`, `UserBucketName`, `ShopBucketName`
  - `ResendClientOptions` — `ApiToken`
  - `OAuthGoogleOptions` — `ClientId`, `ClientSecret`
  - `YandexApiOptions` — `ApiKey`, `BaseUrl`, `TimeoutSeconds`
  - `WebClientUrl` — frontend URL for email confirmation links
  - `ALLOWED_ORIGINS` — env var for production CORS allowlist (comma-separated)
  - `OTEL_EXPORTER_OTLP_ENDPOINT` — env var to enable OTLP export

**Build:**
- `Directory.Packages.props` — central NuGet version manifest
- `global.json` — SDK version pin
- `Makefile` — EF Core migration helpers (`mig-acc`, `mig-shops`, `mig-mod`, `mig-media`, `up-*`)
- Per-service Dockerfiles: `AccountService.Dockerfile`, `ShopsService.Dockerfile`, `ModerationService.Dockerfile`, `MediaService.Dockerfile`, `Gateway.Dockerfile` (all use `mcr.microsoft.com/dotnet/aspnet:10.0` as runtime base)

## Platform Requirements

**Development:**
- .NET 10 SDK (10.0.100+)
- Docker Desktop (for Aspire-managed PostgreSQL 17, RabbitMQ, Redis containers)
- `dotnet workload restore CoffeePeek.slnx` required before first build (Aspire + Android workloads)

**Production:**
- Deployment target: Railway (service internal hostnames `*.railway.internal` in `CoffeePeek.Gateway/appsettings.json`)
- Container images pushed to Docker Hub via GitHub Actions (`docker.io/<DOCKER_HUB_USERNAME>/coffeepeek.*`)
- Target OS: Linux (`DockerDefaultTargetOS=Linux` in all service `.csproj` files)
- PostgreSQL 17 (four separate databases: `account`, `shops`, `moderation`, `media`)

---

*Stack analysis: 2026-05-17*
