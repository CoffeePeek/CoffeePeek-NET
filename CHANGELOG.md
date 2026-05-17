# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v0.3.0] - 2026-05-17

### Added

- **Предложение кофейни** — клиентский сценарий с progressive-form UX: обязательные поля (`название`, `адрес`, `город`) + опциональные секции, раскрываемые по желанию.
- **Загрузка фотографий для предложения кофейни** — presigned upload flow (`api/Photos/shop`) с последующей отправкой на модерацию (`api/ModerationShops`).
- **Создание и удаление отзывов** на странице деталей кофейни: форма оценки (`место/сервис/кофе`) + комментарий, удаление только собственных отзывов.
- **Панель модератора** — отображается в шапке при наличии роли `Moderator` в JWT; очереди кофеен и отзывов с бэка, смена статусов, разбор `role` claim из токена и unit-тесты.
- **Новый HTTP-клиент модерации** в клиентском приложении с unit-тестами для ViewModel и WebClient сценариев.
- **CI/CD: деплой Avalonia Browser (WASM) на Vercel** — workflow публикует `dist/wwwroot` Release-сборки; preview на `dev`, продакшн на `main`.

### Changed

- **Редизайн домашней страницы** — bento-grid layout, обновлённая цветовая схема и design system (`Resources/Styles`, `Resources/Themes`).
- **Обработка ошибок в auth-слое** — встроенные ошибки заменены на `DomainException` во всех обработчиках auth (login, logout, register, refresh, confirm email, Google OAuth).
- **Логика переключения языка** — переработана поддержка мультиязычности в клиентском приложении.
- **AppHost** — Avalonia Desktop/Android исключены из Aspire-оркестрации; проект переориентирован на Browser (WASM).

### Fixed

- Миграции Account DB Context обновлены под актуальную схему.
- Исправлена конфигурация Vercel CLI в CI/CD workflow.
- Удалены `appsettings.Development.json` из auth-сервисных проектов (секреты не должны коммититься).

## [v0.2.1] - 2026-05-09

### Added

- **Goo Trio loader (02)** — organic three-blob loading animation added to all loading pages: shop catalog, shop detail, and user profile. Blobs oscillate in gold/amber palette using CSS-style Canvas animations.
- **Wobble Ring loader (11)** — compact spinning arc shown inside buttons during async operations. Applied to: Submit Review, Save Profile, and Login buttons. Replaces the generic ProgressBar with a themed organic spinner.
- `Controls/GooTrioLoader` and `Controls/WobbleRingLoader` — reusable Avalonia `UserControl`s for page-level and inline button loading states.
- `Resources/Styles/LoaderStyles.axaml` — shared animation styles for both loaders, registered globally in `App.axaml`.

### Fixed

- `ILocalizationService`, `LocalizationService`, `Loc` singleton, and `ApplyPersistedLanguageExecutor` — missing implementations that caused build failure.
- `GetAllModerationReviewsResultDto.Reviews` renamed to `ReviewDtos`; redundant `[JsonPropertyName]` removed.
- `GetAllModerationShopsResultDto.ModerationShop` and server `GetAllModerationShopsResponse` renamed to `ModerationShops` (plural).
- `ChangeReviewStatusAsync` renamed to `UpdateReviewStatusAsync` for consistent verb usage across the moderation client.
- `ClientSession.SetAccessToken` / `Clear` — made thread-safe with `lock` and `Volatile.Read`.
- `JwtRoleParser.CollectRolesFrom` — uses `HashSet<string>` to deduplicate roles from both claim name variants.
- `ShopDetailViewModelTests` updated to match updated constructor signature (`HttpClient`, `ApiOptions`).

## [v0.2.0] - 2026-04-25

### Added

- Централизованное управление версиями API через `ApiVersions.cs`.
- Поддержка версионирования через заголовок `X-Api-Version`.
- Новая версия API `V2.0` для `UsersController` (регистрация, поиск, профиль).
- Расширения Swagger для автоматической генерации документации по версиям.
- `ClaimsPrincipalExtensions` для безопасного извлечения `UserId`.
- Реализован флоу Login с поддержкой JWT и Refresh токенов.
- Добавлен клиентский HTTP pipeline и инфраструктура пользовательского контекста.
- Добавлено клиентское приложение, включая Android-конфигурацию и подключение frontend к Aspire.
- Добавлен клиент кофеен: поиск, каталог городов и отображение реальных данных на странице кофеен.
- Добавлен экран деталей кофейни с расписанием, контактами, отзывами, тегами, рейтингом и навигацией из списка.
- Добавлены локализации для страницы деталей кофейни.
- Добавлены MediaService, outbox/messaging-интеграции и миграции для сервисов Account, Shops, Moderation и Media.

### Changed

- Обновлен `AuthController`: переход на использование новых констант версионирования. Переход к REST-подходу, обновлены все эндпоинты.
- Рефакторинг MediatR команд: `LoginRequest` и `RefreshTokenCommand` обновлены для поддержки новых контрактов.
- Оптимизированы запросы поиска кофеен и получения деталей кофейни.
- Запрос создания отзыва удален из отдельного флоу; логика перенесена в детали кофейни `api/coffeeshops/{shopId}`.
- Обновлена модель Equipment для улучшения specialty-направления, добавлены доменные тесты.
- Разделена логика DI в AccountService, добавлен persistence layer.
- Логика проверки авторизации вынесена в Gateway.
- Общая инфраструктура переведена с MediatR/MassTransit на Wolverine там, где это применимо.
- Обновлены Dockerfile и CI/CD pipeline для сборки сервисов, публикации образов и применения миграций.
- Индикаторы загрузки в клиенте заменены на shimmer skeleton placeholders.

### Fixed

- Исправлена привязка маршрутов в Gateway (YARP) для поддержки версионированных эндпоинтов.
- Исправлены настройки Gateway, auth defaults и auth policy.
- Исправлены проблемы сборки Wolverine assemblies, Dockerfile и тестов.

## [v0.1.2] - 2026-02-02

### Changed

- Модель Equipment, для улучшения specialty, добавлены тесты для доменной сущности 
- Разделена логика DI в AccountService, добавлен Persistance layer
- Вынесена логика проверки Auth в Gateway
- MediaService для централизации всех фотографий

## [v0.1.1] - 2026-01-23

### Fixed

- Исправлена ошибка когда пользователь мог залогиниться с неправильным паролем

## [v0.1.0] - 2026-01-22

### Added

- Начало ведения `Changelog.md`
- Начальная реализация микросервисов: Account, Shops, JobVacancies, Moderation.
- API Gateway на базе YARP.
- Интеграция с .NET Aspire для локальной разработки.