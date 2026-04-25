# CoffeePeek Project Documentation

## Table of Contents
1. [About the Project](#about-the-project)
2. [Architecture Overview](#architecture-overview)
3. [Tech Stack](#tech-stack)
4. [Project Structure](#project-structure)
5. [Getting Started](#getting-started)
6. [Running the Application](#running-the-application)
7. [Development Workflow](#development-workflow)
8. [Database Migrations](#database-migrations)
9. [Testing](#testing)

## About the Project

CoffeePeek is a comprehensive platform that connects coffee enthusiasts with coffee shops. It allows users to discover coffee shops, view job vacancies, and share their experiences. The back-end is built using a microservices architecture to ensure scalability, resilience, and maintainability. Each service is responsible for a specific business domain, communicating with others through a well-defined API gateway.

## Architecture Overview

The CoffeePeek application follows a microservices architecture pattern with the following core services:

- **API Gateway** (YARP-based): Handles routing, authentication, and cross-cutting concerns
- **Account Service**: Manages user accounts, authentication, and authorization
- **Shops Service**: Handles coffee shop discovery, search, and management
- **Moderation Service**: Provides content moderation capabilities (integrates with external services)
- **Media Service**: Manages media assets and file uploads

The architecture also includes shared libraries for common functionality and a distributed app host for local development.

## Tech Stack

- **.NET 10**: The primary framework for building the services
- **ASP.NET Core**: Used for building web APIs
- **YARP (Yet Another Reverse Proxy)**: Powers the API Gateway
- **Entity Framework Core**: Used as the Object-Relational Mapper (ORM) for data access
- **PostgreSQL**: The relational database for the services
- **RabbitMQ**: Used for asynchronous communication between services
- **Redis**: For caching and session management
- **Docker & Docker Compose**: For containerization and orchestration
- **MediatR/Wolverine**: For CQRS pattern implementation
- **xUnit/NUnit**: For unit and integration testing

## Project Structure

```
CoffeePeek-BackEnd/
├── CoffePeek.AppHost/          # Aspire distributed application host
├── CoffePeek.ServiceDefaults/  # Shared service defaults and configurations
├── CoffeePeek.Gateway/         # API Gateway service
├── CoffeePeek.AccountService/  # User account management service
│   ├── CoffeePeek.Account.Application/    # Application layer
│   ├── CoffeePeek.Account.Domain/         # Domain entities and business logic
│   ├── CoffeePeek.Account.Infrastructure/ # Infrastructure implementations
│   └── CoffeePeek.Account.Persistence/    # Database persistence layer
├── CoffeePeek.ShopsService/    # Coffee shops management service
│   ├── CoffeePeek.Shops.Application/
│   ├── CoffeePeek.Shops.Domain/
│   ├── CoffeePeek.Shops.Infrastructure/
│   └── CoffeePeek.Shops.Persistance/
├── CoffeePeek.ModerationService/ # Content moderation service
├── CoffeePeek.MediaService/    # Media handling service
├── CoffeePeek.Contract/        # Shared contracts and DTOs
├── CoffeePeek.Shared.*         # Various shared libraries
└── ...
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Docker Desktop (for containerization)
- PostgreSQL (or Docker for automatic setup)
- Visual Studio 2022 / Rider / VS Code with C# extension

### Installation Steps

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd CoffeePeek-BackEnd
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Set up the development environment:
    - Ensure Docker is running
    - The Aspire AppHost will automatically provision PostgreSQL databases

## Running the Application

### Option 1: Using Aspire (Recommended for Development)

The easiest way to run the entire application locally is using the Aspire AppHost:

```bash
cd CoffePeek.AppHost
dotnet run
```

This will:
- Start PostgreSQL database containers
- Provision databases for all services
- Launch all microservices
- Expose the API Gateway at `http://localhost:5000`
- Provide dashboard and documentation at `http://localhost:5000/scalar`

### Option 2: Running Individual Services

You can run individual services if needed:

```bash
# Run the API Gateway
cd CoffeePeek.Gateway
dotnet run

# Run the Account Service
cd CoffeePeek.AccountService
dotnet run

# Run the Shops Service
cd CoffeePeek.ShopsService
dotnet run

# Run the Moderation Service
cd CoffeePeek.ModerationService
dotnet run

# Run the Media Service
cd CoffeePeek.MediaService
dotnet run
```

### Option 3: Using Docker

Build and run with Docker:

```bash
# Build all services
docker-compose build

# Run all services
docker-compose up -d
```

## Development Workflow

### Adding New Features

1. Identify which service the feature belongs to
2. Follow the layered architecture pattern (Application, Domain, Infrastructure, Persistence)
3. Use CQRS pattern with MediatR/Wolverine
4. Add appropriate validation and error handling
5. Write unit tests for new functionality
6. Update API documentation if adding new endpoints

### Database Schema Changes

1. Make changes to entity models in the Domain layer
2. Add new migrations using the Makefile:
   ```bash
   # For account service
   make mig-acc n=MigrationName
   
   # For shops service
   make mig-shops n=MigrationName
   
   # For moderation service
   make mig-mod n=MigrationName
   
   # For media service
   make mig-media n=MigrationName
   ```

3. Update the database:
   ```bash
   # For account service
   make up-acc
   
   # For shops service
   make up-shops
   
   # For moderation service
   make up-mod
   
   # For media service
   make up-media
   ```

## Database Migrations

The project uses Entity Framework Core for database migrations. Each service has its own database context and migration history.

### Migration Commands (using Makefile)

```bash
# Add a new migration to Account service
make mig-acc n=AddUserPreferences

# Update Account database to latest migration
make up-acc

# Add a new migration to Shops service
make mig-shops n=AddCoffeeEquipment

# Update Shops database to latest migration
make up-shops

# Add a new migration to Moderation service
make mig-mod n=UpdateContentRules

# Update Moderation database to latest migration
make up-mod

# Add a new migration to Media service
make mig-media n=AddImageOptimization

# Update Media database to latest migration
make up-media
```

### Manual Migration Commands

If you prefer to use dotnet ef directly:

```bash
# From the project root
dotnet ef migrations add MigrationName \
  --project CoffeePeek.Account.Persistence \
  --startup-project CoffeePeek.AccountService \
  --context CoffeePeek.Account.Persistence.Configuration.AccountDbContext

# Update database
dotnet ef database update \
  --project CoffeePeek.Account.Persistence \
  --startup-project CoffeePeek.AccountService \
  --context CoffeePeek.Account.Persistence.Configuration.AccountDbContext
```

## Testing

The project includes unit tests for domain logic and infrastructure components:

- `CoffeePeek.Account.Domain.Tests` - Tests for account domain models
- `CoffeePeek.Account.Infrastructure.Tests` - Tests for account infrastructure
- `CoffeePeek.Shops.Domain.Tests` - Tests for shops domain models

Run all tests:
```bash
dotnet test
```

Run tests for a specific project:
```bash
dotnet test CoffeePeek.Account.Domain.Tests
```

## API Documentation

The API Gateway includes Swagger/OpenAPI documentation accessible at:
- `http://localhost:5000/scalar` (Scalar UI - modern alternative to Swagger UI)
- `http://localhost:5000/swagger` (Traditional Swagger UI)

API endpoints are organized by service:
- `/api/account/*` - Account management endpoints
- `/api/shops/*` - Coffee shop discovery endpoints
- `/api/moderation/*` - Content moderation endpoints
- `/api/media/*` - Media handling endpoints