# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Централизованное управление версиями API через `ApiVersions.cs`.
- Поддержка версионирования через заголовок `X-Api-Version`.
- Новая версия API `V2.0` для `UsersController` (регистрация, поиск, профиль).
- Расширения Swagger для автоматической генерации документации по версиям.
- `ClaimsPrincipalExtensions` для безопасного извлечения `UserId`.

### Changed
- Обновлен `AuthController`: переход на использование новых констант версионирования. Переход к REST-подходу, обновлены все эндпоинты.

### Fixed
- Исправлена привязка маршрутов в Gateway (YARP) для поддержки версионированных эндпоинтов.

## [v0.1.1] - 2026-01-22

### Added
- Начало ведения `Changelog.md`
- Начальная реализация микросервисов: Account, Shops, JobVacancies, Moderation.
- API Gateway на базе YARP.
- Интеграция с .NET Aspire для локальной разработки.
