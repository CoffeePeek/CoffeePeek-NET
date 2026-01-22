# CoffeePeek Back-End

This repository contains the back-end source code for the CoffeePeek application, a microservices-based platform designed to connect coffee enthusiasts with coffee shops.

## About The Project

CoffeePeek is a comprehensive platform that allows users to discover coffee shops, view job vacancies, and share their experiences. The back-end is built using a microservices architecture to ensure scalability, resilience, and maintainability. Each service is responsible for a specific business domain, communicating with others through a well-defined API gateway.

## Документация

- [Changelog](CHANGELOG.md) — история изменений (фичи, баги, рефакторинг).
- [Domain Map](docs/domain-map.md) — описание DDD доменов и связей.
- [Architecture Decisions (ADR)](docs/adr/template.md) — реестр важных технических решений.

## Структура проекта

Проект организован по принципам Clean Architecture и DDD в рамках каждого микросервиса:

- `CoffeePeek.[DomainName].Domain`: Ядро домена (Entities, Value Objects, Domain Events).
- `CoffeePeek.[DomainName].Application`: Логика приложения (Commands, Queries, Handlers, DTOs).
- `CoffeePeek.[DomainName].Infrastructure`: Реализация внешних интерфейсов (DB Context, Repositories, API Clients).
- `CoffeePeek.[DomainName]Service`: Точка входа (ASP.NET Core Controllers/Minimal API).
- `CoffeePeek.Shared.*`: Общий код для всех сервисов.

## Как запустить

1. Установите .NET 10 SDK.
2. Запустите проект через Aspire AppHost (`CoffePeek.AppHost`).
3. Документация API будет доступна через Swagger UI для каждого сервиса.

## Tech Stack

The project is built with a modern technology stack:

*   **[.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)**: The primary framework for building the services.
*   **[ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)**: Used for building web APIs.
*   **[YARP (Yet Another Reverse Proxy)](https://microsoft.github.io/reverse-proxy/)**: Powers the API Gateway.
*   **[Docker](https://www.docker.com/)**: The services are containerized for easy deployment and scaling.
*   **Microservices Architecture**: The overall design pattern for the system.
*   **Entity Framework Core**: Used as the Object-Relational Mapper (ORM) for data access.
*   **PostgreSQL**: The relational database for the services.
*   **RabbitMQ**: Used for asynchronous communication between services.
*   **xUnit/NUnit**: For unit and integration testing.
