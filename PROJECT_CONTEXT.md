# PROJECT_CONTEXT.md

Черновик контекста проекта CoffeePeek.NET для разработчиков и AI-агентов.

## Краткое описание

CoffeePeek.NET - модульный .NET 10 монорепозиторий для CoffeePeek. В репозитории есть:

- backend-сервисы Account, Shops, Moderation и Media;
- Gateway на YARP;
- Aspire AppHost для локальной оркестрации;
- shared-библиотеки для auth, web, persistence, domain, validation и contracts;
- мультиплатформенный Avalonia-клиент с MVVM, Autofac DI и платформенными хостами Desktop, Browser, Android.

Решение описано в `CoffeePeek.slnx`. Классического `*.sln` в корне не обнаружено.

## Solution Layout

Основные логические группы в `CoffeePeek.slnx`:

- `Client` - общий Avalonia-клиент и клиентские слои.
- `Client/Platforms` - платформенные хосты Avalonia: Android, Browser, Desktop.
- `CoffeePeek.Development` - Aspire AppHost и ServiceDefaults.
- `CoffeePeek` - Gateway и MediaService.
- `CoffeePeek/CoffeePeek.Account` - Account bounded context.
- `CoffeePeek/CoffeePeek.Shops` - Shops bounded context.
- `CoffeePeek/CoffeePeek.Moderation` - Moderation bounded context.
- `CoffeePeek/Shared` - общие библиотеки.
- `Solution Items` - `Directory.Packages.props`, `Makefile`, `CHANGELOG.md`, CI workflow.

## Central Package Management

Зависимости централизованы в `Directory.Packages.props` через:

```xml
<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
```

Версии сгруппированы по секциям:

- `Shared` - Autofac, EF Core, Npgsql, Wolverine, Redis, FluentResults, Polly, Serilog, Sentry, Mapster, OpenTelemetry.
- `Account` - Google auth, Resend, hashing.
- `Development.Aspire` - Aspire hosting packages.
- `Media` - Minio.
- `Gateway` - Scalar, YARP.
- `Tests` - xUnit v3, FluentAssertions, Moq, coverlet.
- `Client.Configuration` - Microsoft.Extensions.Configuration packages.
- `Avalonia` - Avalonia, CommunityToolkit.Mvvm, platform packages.

Большинство проектов таргетят `net10.0`, используют `Nullable` и `ImplicitUsings`. `CoffeePeek.Client.App` также включает `LangVersion=preview` и compiled bindings для Avalonia.

## Backend Architecture

Backend устроен по bounded contexts с привычным разбиением на слои:

- `CoffeePeek.Account.Domain`
- `CoffeePeek.Account.Application`
- `CoffeePeek.Account.Infrastructure`
- `CoffeePeek.Account.Persistence`
- `CoffeePeek.AccountService`

Аналогичный паттерн используется для Shops и Moderation:

- `CoffeePeek.Shops.Domain`, `Application`, `Infrastructure`, `Persistance`, `CoffeePeek.ShopsService`
- `CoffeePeek.Moderation.Domain`, `Application`, `Infrastructure`, `CoffeeShop.Moderation.Persistence`, `CoffeePeek.ModerationService`

MediaService устроен проще и живет как отдельный host project с зависимостями на auth/persistence и Minio.

### Service Hosts

Сервисные host-проекты используют `Program.cs` как тонкую точку входа:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddApplication();

var app = builder.Build();

app.UseApplication();

app.Run();
```

Основная сборка middleware и DI вынесена в `InfrastructureExtensions.cs` и `DependencyInjection.cs`.

Типичный порядок для сервисов:

- `builder.AddSerilogLogging()`;
- `builder.AddServiceDefaults()`;
- `builder.WebHost.ConfigureWebhost()`;
- `builder.AddWolverine(...)`;
- `services.AddApplication().AddPersistence(...).AddInfrastructure().AddPresentation()`;
- `app.UseSerilogRequestLogging()`;
- `app.UseExceptionHandler()`;
- auth/authorization там, где нужно;
- OpenAPI только в Development;
- `app.MapDefaultEndpoints()`;
- `app.MapControllers()`.

## Aspire And Infrastructure

`CoffePeek.AppHost` использует Aspire AppHost SDK и поднимает:

- PostgreSQL с базами `AccountDb`, `ShopsDb`, `ModerationDb`, `MediaDb`;
- внешний Vite frontend из `../../CoffeePeek.FE.WebClient/coffee-peek`;
- AccountService, ShopsService, ModerationService, MediaService;
- Gateway с reference на downstream services и URL `/scalar` для docs.

`CoffePeek.ServiceDefaults` содержит общие Aspire defaults:

- service discovery;
- HTTP resilience;
- health checks;
- OpenTelemetry metrics/tracing;
- OTLP exporter при наличии `OTEL_EXPORTER_OTLP_ENDPOINT`.

Важно: в именах есть текущие несогласованности:

- `CoffePeek.*` вместо `CoffeePeek.*` для AppHost и ServiceDefaults;
- `CoffeePeek.Shops.Persistance` с опечаткой в `Persistence`;
- `CoffeeShop.Moderation.Persistence` с префиксом `CoffeeShop`, а не `CoffeePeek`.

## Gateway

`CoffeePeek.Gateway` - ASP.NET Core gateway на YARP.

Основные зависимости:

- `Yarp.ReverseProxy`;
- `Microsoft.Extensions.ServiceDiscovery.Yarp`;
- `Scalar.AspNetCore`;
- `CoffeePeek.Shared.Auth`;
- `CoffePeek.ServiceDefaults`.

Gateway отвечает за:

- reverse proxy routing из `appsettings.json`;
- service discovery destination resolver;
- JWT auth на edge;
- CORS;
- rate limiting;
- response caching;
- request logging middleware;
- Scalar/OpenAPI aggregation surface.

В коде есть явная установка Kestrel request limits и отдельный health endpoint `/health/gateway`.

## Messaging And Persistence

В backend активно используется Wolverine:

- handlers находятся в `Application/Features/...`;
- команды и query обычно представлены `record`-типами с суффиксами `Command` или `Query`;
- handler-методы часто оформлены как `static Handle(...)`;
- Wolverine discovery подключается на уровне service host через массив assemblies.

`CoffeePeek.Shared.Persistence` содержит общую интеграцию:

- Wolverine;
- Wolverine.EntityFrameworkCore;
- Wolverine.Postgresql;
- Wolverine.RabbitMQ;
- EF Core transactions;
- PostgreSQL message persistence;
- RabbitMQ auto provisioning;
- Redis/cache integration.

Persistence-проекты bounded contexts регистрируют DbContext, unit of work, query services и repositories через `IServiceCollection`.

## Shared Projects

Роль shared-проектов:

- `CoffeePeek.Shared.Kernel` - базовые primitives/options/results, включая `FluentResults`.
- `CoffeePeek.Contract` - общие contracts/events/DTO.
- `CoffeePeek.Shared.Domain` - domain abstractions.
- `CoffeePeek.Shared.Validation` - validation abstractions/extensions.
- `CoffeePeek.Shared.Persistence` - EF/Wolverine/PostgreSQL/RabbitMQ/Redis integration.
- `CoffeePeek.Shared.Web` - OpenAPI, API versioning, CORS/environment modules, exception handling, Serilog/Sentry helpers.
- `CoffeePeek.Shared.Auth` - JWT bearer auth и header-based user context.

## Client Architecture

Клиентская часть построена вокруг Avalonia:

- `CoffeePeek.Client.App` - общий UI, XAML resources, views, view models, services, startup pipeline.
- `CoffeePeek.Client.App.Core` - абстракции для execution, session/settings/identity.
- `CoffeePeek.Client.App.Infrastructure` - configuration, local settings, Autofac helpers, web client implementations.
- `CoffeePeek.Client.App.Infrastructure.HTTP` - HTTP pipeline, token refresh, web client interfaces, DTO/responses.
- `CoffeePeek.Client.App.Contract` - клиентские contracts.
- `CoffeePeek.Client.App.Desktop`, `Browser`, `Android` - платформенные entry points.

`CoffeePeek.Client.App` зависит только от `CoffeePeek.Client.App.Infrastructure`, а платформенные проекты зависят от общего `CoffeePeek.Client.App`.

## MVVM Pattern

MVVM реализован через Avalonia и CommunityToolkit.Mvvm:

- `ViewModelBase` наследуется от `ObservableObject`;
- ViewModels используют `[ObservableProperty]` и `[RelayCommand]`;
- Views находятся в `Views/...`;
- ViewModels находятся в `ViewModels/...`;
- XAML использует `x:DataType` для compiled bindings;
- `App.axaml` регистрирует `ViewLocator` в `Application.DataTemplates`.

`ViewLocator` мапит view model в view по соглашению:

- `FooViewModel` -> `FooView`;
- замена происходит по `FullName.Replace("ViewModel", "View")`;
- view создается через reflection и `Activator.CreateInstance`.

Это означает, что namespace и структура папок для Views/ViewModels должны оставаться параллельными.

## Client DI

DI на клиенте построен на Autofac.

Composition root:

- `Bootstrapper.BuildContainer()`;
- `ApplicationModule`;
- `InfrastructureModule`;
- `HttpModule`;
- `UiServiceModule`.

Типичный стиль регистрации:

- ViewModels регистрируются `AsSelf().SingleInstance()`;
- сервисы регистрируются через интерфейсы;
- `NavigationService` получает factory `Func<Type, ViewModelBase>`, которая резолвит ViewModel из Autofac scope;
- HTTP pipeline регистрируется вручную как упорядоченный список behaviors/steps.

`HttpModule` регистрирует:

- `HttpClient`;
- `ITokenRefresher`;
- все `IHeaderSetter` через assembly scanning;
- `HttpPipeline`;
- `IHttpCommandExecutor`;
- `IWebAuthenticationClient`;
- `IWebUsersClient`;
- `IWebUserProfileClient`;
- `IWebUserReviewsClient`;
- `IUserIdentityAccessor`.

## Client Navigation And Startup

Навигация:

- `INavigationService` хранит один `CurrentView`;
- `NavigateTo<T>()` резолвит ViewModel и показывает ее как активный flow;
- `Reset()` очищает flow;
- `HasActiveFlow` и `IsMainChromeVisible` управляют отображением shell/overlay в `MainView.axaml`.

Startup pipeline:

- `IApplicationExecutorRunner`;
- `IAfterInitExecutor`;
- `IBeforeMainShellExecutor`;
- `IAfterStartupExecutor`;
- executors имеют `Order` и запускаются по фазам.

Примеры:

- `RestoreSessionExecutor`;
- `InitialRouteExecutor`.

## Resources And Styling

Avalonia resources организованы в `CoffeePeek.Client.App/Resources`:

- `Themes/LightColors.axaml`;
- `Themes/DarkColors.axaml`;
- `Styles/TextStyles.axaml`;
- `Styles/TextBoxStyles.axaml`;
- `Styles/ButtonStyles.axaml`;
- `Styles/GeneralStyles.axaml`;
- `Styles/ShopsPageStyles.axaml`;
- `Icons/AppIcons.axaml`;
- `Constants.axaml`;
- `Lang/Resources.resx`;
- `Lang/Resources.ru.resx`;
- generated `Resources.Designer.cs`.

`App.axaml` подключает FluentTheme, theme dictionaries, fonts, constants, icons и style includes.

## Naming Conventions

Общие наблюдения:

- проекты в основном называются `CoffeePeek.<Area>.<Layer>`;
- сервисные host-проекты называются `CoffeePeek.<Area>Service`;
- shared-проекты называются `CoffeePeek.Shared.<Concern>`;
- интерфейсы имеют префикс `I`;
- async-методы имеют суффикс `Async`;
- commands/queries/handlers лежат в `Application/Features/<Feature>/<UseCase>`;
- response DTO часто имеют суффикс `Response`, клиентские HTTP DTO - `Dto`;
- Autofac modules имеют суффикс `Module`;
- ASP.NET Core DI entry points обычно называются `DependencyInjection` или `InfrastructureExtensions`;
- ViewModels имеют суффикс `ViewModel`, Views - `View`, сервисы - `Service`, контроллеры - `Controller`.

Имена с потенциальной технической задолженностью:

- `CoffePeek` vs `CoffeePeek`;
- `Persistance` vs `Persistence`;
- `CoffeeShop.Moderation.Persistence` vs остальной namespace prefix `CoffeePeek`.

## Testing

Тестовые проекты есть для:

- Account Application;
- Account Domain;
- Account Infrastructure;
- Shops Domain.

Основные тестовые зависимости:

- xUnit v3;
- FluentAssertions;
- Moq;
- coverlet.collector;
- Microsoft.NET.Test.Sdk;
- JetBrains.Annotations.

## Notes For Future Contributors

- Добавляя backend feature, сначала ищите соответствующий bounded context и папку `Application/Features`.
- Для application handlers придерживайтесь существующего Wolverine-style `Command/Query` + `Handle(...)`.
- Для backend DI используйте существующие extension methods и `DependencyInjection.cs`, а не ad-hoc регистрацию в `Program.cs`.
- Для persistence используйте существующие `AddPersistence(...)`, repositories, query services и unit of work patterns.
- Для client UI сохраняйте параллельную структуру `Views/...` и `ViewModels/...`, иначе `ViewLocator` не найдет view.
- Для клиентских сервисов регистрируйте зависимости в Autofac modules.
- Для новых client HTTP endpoints добавляйте interface в `Infrastructure.HTTP/WebClients` и implementation в `Infrastructure/WebClient`.
- Для строк UI используйте `.resx` resources, а не hardcoded text в XAML/ViewModels.
- Для зависимостей добавляйте версию в `Directory.Packages.props`, а в `.csproj` оставляйте `PackageReference` без версии.

## Source Note

Запрос предполагал использование GitHub MCP. В текущей Cursor-конфигурации сервер `github` виден, но callable tool-дескрипторов для чтения репозитория не обнаружено, поэтому черновик подготовлен по локальной рабочей копии репозитория и доступным MCP metadata.
