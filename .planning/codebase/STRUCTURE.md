# Structure Map
*Last mapped: 2026-05-17*

## Root Layout

Flat structure — 30+ projects at root level (no nested solution folders). Grouped by bounded context prefix.

```
/
├── CoffeePeek.slnx                          — Solution file
├── Directory.Packages.props                  — Central NuGet package management
├── Makefile                                  — EF migration shortcuts
├── .github/workflows/ci-cd.yml              — CI/CD pipeline
│
├── CoffeePeek.Gateway/                      — YARP reverse proxy + Scalar docs
│
├── CoffeePeek.Account.Domain/               — Account bounded context
├── CoffeePeek.Account.Application/
├── CoffeePeek.Account.Infrastructure/
├── CoffeePeek.Account.Persistence/
├── CoffeePeek.AccountService/               — Service host
│
├── CoffeePeek.Shops.Domain/                 — Shops bounded context
├── CoffeePeek.Shops.Application/
├── CoffeePeek.Shops.Infrastructure/
├── CoffeePeek.Shops.Persistance/            — ⚠ typo: Persistance (not Persistence)
├── CoffeePeek.ShopsService/                 — Service host
│
├── CoffeePeek.Moderation.Domain/            — Moderation bounded context
├── CoffeePeek.Moderation.Application/
├── CoffeePeek.Moderation.Infrastructure/
├── CoffeeShop.Moderation.Persistence/       — ⚠ wrong prefix: CoffeeShop (not CoffeePeek)
├── CoffeePeek.ModerationService/            — Service host
│
├── CoffeePeek.MediaService/                 — Media service (all-in-one, no layer split)
│
├── CoffeePeek.Shared.Kernel/                — Shared libraries
├── CoffeePeek.Shared.Domain/
├── CoffeePeek.Shared.Persistence/
├── CoffeePeek.Shared.Auth/
├── CoffeePeek.Shared.Web/
├── CoffeePeek.Contract/
│
├── CoffePeek.AppHost/                       — ⚠ typo: CoffePeek (Aspire orchestration)
├── CoffePeek.ServiceDefaults/               — ⚠ typo: CoffePeek (health checks, OTel)
│
├── CoffeePeek.Client.App/                   — Avalonia client
├── CoffeePeek.Client.App.Core/
├── CoffeePeek.Client.App.Infrastructure/
├── CoffeePeek.Client.App.Infrastructure.HTTP/
├── CoffeePeek.Client.App.Desktop/
├── CoffeePeek.Client.App.Browser/
├── CoffeePeek.Client.App.Android/
│
└── CoffeePeek.Account.Domain.Tests/         — Test projects
```

## Naming Conventions

| Pattern | Example |
|---------|---------|
| Bounded context library | `CoffeePeek.<Context>.<Layer>` |
| Service host | `CoffeePeek.<Context>Service` |
| Shared library | `CoffeePeek.Shared.<Concern>` |
| Test project | `CoffeePeek.<Context>.<Layer>.Tests` |
| Client project | `CoffeePeek.Client.App.<Platform>` |

## Known Accepted Typos

These typos exist in folder/project names and are **intentional** (do not rename):
- `CoffePeek.*` — AppHost, ServiceDefaults (missing one `e`)
- `CoffeePeek.Shops.Persistance` — missing `s` in Persistence
- `CoffeeShop.Moderation.Persistence` — wrong prefix (`CoffeeShop` vs `CoffeePeek`)

## Application Layer Structure

```
Application/
├── Features/
│   ├── <Feature>/
│   │   ├── <UseCase>/
│   │   │   ├── <UseCase>Command.cs   (or Query)
│   │   │   └── <UseCase>Handler.cs
│   │   └── ...
│   └── ...
├── Common/
├── Extensions/
├── Mapper/
├── Services/
├── ValidationStrategy/
└── DependencyInjection.cs
```

## Service Host Structure

```
<Context>Service/
├── Program.cs                    — thin entry point (AddApplication → Build → UseApplication → Run)
├── InfrastructureExtensions.cs   — all service registration
├── DependencyInjection.cs        — Wolverine assembly scanning
└── appsettings.json
```

## Client Structure

```
CoffeePeek.Client.App/
├── Views/                        — XAML views (parallel to ViewModels)
├── ViewModels/                   — MVVM view models
├── Resources/
│   ├── Themes/                   — Light/Dark themes
│   ├── Styles/
│   └── Resources.resx            — Localization (+ Resources.ru.resx)
└── App.axaml
```

## Where to Add New Code

| What | Where |
|------|-------|
| New feature handler | `<Context>.Application/Features/<Feature>/<UseCase>/` |
| New domain entity | `<Context>.Domain/Entities/` or `Aggregates/` |
| New repository | `<Context>.Infrastructure/` (interface) + `<Context>.Persistence/` (impl) |
| New API route | `Gateway/appsettings.json` under `ReverseProxy` |
| New EF migration | `make mig-<context> n=MigrationName` |
| New shared contract | `CoffeePeek.Contract/` |
