# Testing Map
*Last mapped: 2026-05-17*

## Framework

| Tool | Version | Purpose |
|------|---------|---------|
| xunit.v3 | central | Test runner |
| FluentAssertions | central | Assertion DSL |
| Moq | central | Mocking |
| coverlet.collector | central | Code coverage (XPlat Code Coverage) |
| Microsoft.NET.Test.Sdk | central | Build/discovery support |

## Test Projects

| Project | Scope |
|---------|-------|
| `CoffeePeek.Account.Domain.Tests` | Domain entity/aggregate unit tests |
| `CoffeePeek.Account.Application.Tests` | Application handler/filter unit tests |
| `CoffeePeek.Account.Infrastructure.Tests` | Infrastructure adapter tests |
| `CoffeePeek.Shops.Domain.Tests` | Shops domain entity tests |
| `CoffeePeek.Client.App.Tests` | Client/UI tests |
| `CoffeePeek.Backend.Tests` | Backend integration tests |

**No test projects exist for:** Moderation, Media, Gateway, Shared libraries.

## Test Structure Pattern

Domain tests are flat (files at root of test project):
```
CoffeePeek.Account.Domain.Tests/
├── EmailTests.cs
├── PhoneNumberTests.cs
├── PhotoMetadataTests.cs
├── RefreshTokenTests.cs
├── RefreshTokenAdditionalTests.cs
├── UsernameTests.cs
├── UserCredentialTests.cs
└── UserTests.cs
```

Application tests use feature folders:
```
CoffeePeek.Account.Application.Tests/
├── GlobalUsings.cs
└── Common/
    └── EmailExistenceFilterTests.cs
```

Shops domain tests use entity/aggregate folders:
```
CoffeePeek.Shops.Domain.Tests/
└── Entities/
    └── CoffeeShopAggregate/
        └── EquipmentTest.cs
```

## Test Anatomy

Standard Arrange / Act / Assert with FluentAssertions:

```csharp
[Fact]
public void Register_WithValidParameters_ShouldCreateUser()
{
    // Arrange
    const string email = "test@example.com";
    const string username = "testuser";
    const string passwordHash = "hashed_password";

    // Act
    var user = User.Register(email, username, passwordHash, Role.Create("User"));

    // Assert
    user.Should().NotBeNull();
    user.Id.Should().NotBe(Guid.Empty);
    user.Username.Value.Should().Be(username);
    user.IsSoftDelete.Should().BeFalse();
}
```

Test method naming: `Method_Scenario_ExpectedResult` (e.g. `Register_WithValidParameters_ShouldCreateUser`).

## Running Tests

```bash
dotnet test CoffeePeek.slnx                             # all tests
dotnet test CoffeePeek.Account.Domain.Tests             # specific project
dotnet test CoffeePeek.slnx --filter Name=TestMethodName
dotnet test CoffeePeek.slnx --collect:"XPlat Code Coverage"
```

## Coverage Gaps

- **Shops** — only `EquipmentTest.cs` in domain tests; Application, Infrastructure, Persistence layers have no tests
- **Moderation** — no test projects at all
- **Media** — no test projects at all
- **Gateway** — no test projects at all
- **Shared libraries** — no test projects at all
- **Integration tests** — `CoffeePeek.Backend.Tests` exists but scope unclear; no evidence of Aspire test host or TestContainers usage

## Mocking Strategy

Moq used for interface mocking (repositories, unit of work, external services). No evidence of in-memory EF Core or test database — Moq replaces repositories directly in unit tests.
