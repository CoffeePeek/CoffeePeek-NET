# CoffeePeek.AuthService.Tests

This project contains comprehensive unit tests for the CoffeePeek AuthService.

## Test Coverage

### Services
- `PasswordHasherServiceTests`: Tests password hashing and verification
- Covers various edge cases including special characters, unicode, empty inputs

### Validation
- `UserCreateValidationStrategyTests`: Tests user registration validation
- Email format validation
- Password length constraints
- Edge cases and boundary conditions

### Handlers
- `LoginUserHandlerTests`: Tests user login workflow
- `RegisterUserHandlerTests`: Tests user registration workflow
- Cache integration
- Event publishing
- Error handling

## Running Tests

```bash
dotnet test
```

## Test Approach

- Uses xUnit as the testing framework
- Moq for mocking dependencies
- FluentAssertions for readable assertions
- In-memory database for Entity Framework tests
- Follows AAA (Arrange-Act-Assert) pattern