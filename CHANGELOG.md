# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Добавлен клиентский сценарий `Предложить кофейню` с ненавязчивой progressive-form UX: сначала обязательные поля (`название`, `адрес`, `город`), расширенные секции раскрываются по желанию пользователя.
- Добавлена загрузка фотографий для предложения кофейни через presigned upload flow (`api/Photos/shop`) с последующей отправкой на модерацию (`api/ModerationShops`).
- Добавлен новый HTTP-клиент модерации в клиентском приложении и покрытие unit-тестами для ViewModel и WebClient сценариев.
- Добавлена панель модератора: отображается в шапке при наличии роли `Moderator` в JWT, загрузка очередей кофеен и отзывов с бэка (`api/ModerationShops`, `api/ModerationReviews`), смена статусов, разбор `role` claim из токена и unit-тесты.

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