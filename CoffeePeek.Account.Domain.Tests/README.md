# CoffeePeek Account Domain Tests

This test project contains comprehensive unit tests for the CoffeePeek Account Domain layer.

## Test Coverage

### Value Objects
- **EmailTests**: Tests for Email value object validation, normalization, and equality
- **PhoneNumberTests**: Tests for PhoneNumber validation, formatting, and Belarusian operator detection
- **UsernameTests**: Tests for Username validation, length constraints, and allowed characters
- **UserCredentialTests**: Tests for credential creation, email confirmation, and OAuth provider linking
- **PhotoMetadataTests**: Tests for photo metadata creation and file size validation

### Entities
- **UserTests**: Tests for User registration, profile updates, session management, and role assignment
- **RefreshTokenTests**: Tests for token lifecycle, expiration, and revocation

## Test Framework

- **xUnit**: Primary testing framework
- **FluentAssertions**: For expressive assertions
- **Moq**: For mocking dependencies

## Running Tests

```bash
# Run all tests in this project
dotnet test CoffeePeek.Account.Domain.Tests/CoffeePeek.Account.Domain.Tests.csproj

# Run with coverage
dotnet test CoffeePeek.Account.Domain.Tests/CoffeePeek.Account.Domain.Tests.csproj --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~EmailTests"
```

## Test Patterns

### Value Object Tests
- Validation of valid inputs
- Rejection of invalid inputs with appropriate exceptions
- Edge case handling (min/max lengths, boundaries)
- Equality and hashing behavior
- Implicit conversions

### Entity Tests
- Factory method behavior
- Domain logic correctness
- Invariant enforcement
- Domain event raising
- State transitions

## Key Testing Principles

1. **Arrange-Act-Assert**: All tests follow AAA pattern
2. **Descriptive Names**: Test names clearly describe the scenario and expected outcome
3. **Theory Tests**: Use `[Theory]` with `[InlineData]` for parameterized tests
4. **Exception Testing**: Verify both that exceptions are thrown and their messages
5. **Time-Sensitive Tests**: Use `BeCloseTo` for DateTime assertions to handle timing variations

## Notes

- Tests are independent and can run in any order
- No external dependencies (databases, APIs, etc.)
- Fast execution (all tests should complete in < 5 seconds)
- Domain exceptions from `CoffeePeek.Shared.Extensions.Exceptions` are expected for invalid inputs