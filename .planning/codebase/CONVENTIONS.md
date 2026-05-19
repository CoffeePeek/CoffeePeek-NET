# Coding Conventions

**Analysis Date:** 2026-05-17

## Naming Patterns

**Files:**
- Classes: `PascalCase` matching class name, one class per file — `RegisterUserHandler.cs`, `UserRepository.cs`
- Test files: `{ClassName}Tests.cs` or `{ClassName}Test.cs` (both exist; `Tests` suffix is preferred)
- Commands/Queries: `{Action}{Entity}Command.cs` — `RegisterUserCommand.cs`, `UpdateProfileAboutCommand.cs`
- Handlers: `{Action}{Entity}Handler.cs` — `RegisterUserHandler.cs`, `LoginUserHandler.cs`
- Responses: `{Entity}Response.cs` or `{Action}EntityResponse.cs` — `LoginResponse.cs`, `CreateEntityResponse.cs`

**Classes and Interfaces:**
- Interfaces: `I` prefix + PascalCase — `IUserRepository`, `IUnitOfWork`, `IJWTTokenService`
- Concrete classes: PascalCase, no prefix — `UserRepository`, `CachedUserRepository`
- Static handler classes use `public static class` — `RegisterUserHandler`, `LoginUserHandler`
- Non-static handler classes (when class-level constructor injection needed) use `public class` — `LoginUserHandler`, `AuthService`
- Value objects: `record` type with `Create(...)` factory method — `Email`, `Username`, `PhoneNumber`

**Methods:**
- PascalCase — `RegisterAsync`, `GetById`, `SaveChangesAsync`
- Handler entry point is always named `Handle` — `public static async Task<T> Handle(...)`
- Factory methods on domain objects named `Create` or `Register` — `Email.Create(...)`, `User.Register(...)`

**Variables and Parameters:**
- camelCase for locals and parameters — `userId`, `passwordHash`, `cancellationToken`
- `_camelCase` for private readonly fields — `_userRepoMock`, `_unitOfWorkMock`, `_refreshTokens`
- Constants: `PascalCase` in static classes — `BusinessConstants.MaxActiveSessions`
- CancellationToken parameter conventionally named `ct` in handlers, `cancellationToken` in other methods

**Namespaces:**
- Mirror directory structure — `CoffeePeek.Account.Application.Features.Auth.RegisterUser`
- Test namespace mirrors production namespace — `CoffeePeek.Account.Application.Tests.Features.Auth.Register`

## Code Style

**Formatting:**
- No `.editorconfig` or `.prettierrc` found — enforced via IDE (Rider) settings
- Standard C# formatting: 4-space indentation, opening braces on same line
- `using` file-level namespace declarations (`namespace X;` not `namespace X { }`)
- Trailing semicolons on single-line records

**Nullability:**
- `<Nullable>enable</Nullable>` in all production and test projects — nullable reference types enforced
- Nullable return types annotated with `?` — `Task<User?>`, `string?`
- Null-forgiving operator `!` used sparingly for post-condition guarantees — `user.Credentials.EmailConfirmationToken!`

**Language Features:**
- C# 14 / .NET 10 — use modern features: `record`, collection expressions `[]`, pattern matching
- Target-typed `new()` not used; prefer explicit `new ClassName(...)` or static factory
- `partial record` for source-generated types — `public partial record Email` using `[GeneratedRegex]`
- Primary constructors for services — `public class UserRepository(AccountDbContext dbContext)`
- Init-only properties on response/DTO types — `public bool IsSuccess { get; init; }`

## Import Organization

**Order (observed pattern):**
1. `System.*` namespaces
2. `Microsoft.*` namespaces
3. Third-party packages (`FluentAssertions`, `Moq`, `Wolverine`, `Mapster`)
4. Internal project references (`CoffeePeek.*`)

**Implicit Usings:**
- `<ImplicitUsings>enable</ImplicitUsings>` — standard `System` namespaces auto-imported
- `global using` in test projects for frequently used type aliases — `global using DomainUser = CoffeePeek.Account.Domain.Entities.UserAggregate.User;`
- `<Using Include="Xunit"/>` in some test `.csproj` files to avoid repeating `using Xunit;` in every file

**Path Aliases:**
- None — all imports are fully qualified namespace paths

## Command / Record Design

**Commands and Queries are C# records:**
```csharp
public record RegisterUserCommand(string UserName, string Email, string Password);

public record UpdateProfileAboutCommand(
    [property: JsonIgnore] Guid UserId,
    [property: MaxLength(600)] string About);
```

**Conventions:**
- `[property: JsonIgnore]` on fields that come from the authenticated user context, not the request body — `UserId`, `DeviceName`, `IpAddress`
- `[property: MaxLength(N)]` directly on record properties for validation hints
- Controller merges context into command with `record with` expression: `command = request with { UserId = userContext.GetUserIdOrThrow() };`

## Wolverine Handler Pattern

**Static handler with injected dependencies:**
```csharp
public static class RegisterUserHandler
{
    public static async Task<(CreateEntityResponse, UserRegisteredInternalEvent)> Handle(
        RegisterUserCommand request,
        IQueryUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasherService passwordHasher,
        EmailExistenceFilter emailExistenceFilter,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    { ... }
}
```

**Rules:**
- First parameter is always the command/query message
- Dependencies injected as subsequent parameters by Wolverine
- `CancellationToken ct` is the last parameter
- Handlers can return tuples `(TResponse, TEvent)` to publish side-effect events alongside returning a result
- Handlers are `public static class` when no instance state is needed; `public class` when constructor injection is required (rare)
- Handler invoked from controllers via `IMessageBus.InvokeAsync<TResult>(command)`

## Error Handling

**Exception hierarchy** (`CoffeePeek.Shared.Kernel.Exceptions`):
```
BaseException (abstract)
├── DomainException         — business rule violations, maps to HTTP 400
├── NotFoundException       — entity not found, maps to HTTP 404
├── UnauthorizedException   — auth failures, maps to HTTP 401
├── ValidationException     — input validation, maps to HTTP 400, carries Errors dict
├── ConflictException       — concurrent write conflicts, maps to HTTP 409
└── DatabaseException       — DB failures, maps to HTTP 503
```

**Pattern — throw domain exceptions from handlers:**
```csharp
var user = await userRepository.GetById(command.UserId, ct);
if (user == null)
    throw new NotFoundException("User not found");
```

**Pattern — throw from domain entities:**
```csharp
private Email(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        throw new DomainException("Email cannot be empty.");
    ...
}
```

**Pattern — catch ConflictException for race conditions:**
```csharp
try
{
    await unitOfWork.SaveChangesAsync(ct);
}
catch (ConflictException)
{
    throw new DomainException("Email already exists");
}
```

**Global exception handler** (`CoffeePeek.Shared.Web/Handlers/GlobalExceptionHandler.cs`):
- Registered with `services.AddExceptionHandler<GlobalExceptionHandler>()`
- Maps `BaseException.StatusCode` → HTTP status code
- In DEBUG: includes `StackTrace` and `InnerException`
- In Release: returns safe `"An unexpected error occurred."` for non-domain exceptions
- Returns `ErrorResponse` JSON body

**Do NOT use FluentResults** — although it appears in `Directory.Packages.props`, the codebase uses the custom `Response<T>` / `Response` / `CreateEntityResponse` pattern, not `FluentResults`. Do not introduce `Result<T>` from FluentResults.

## Response Pattern

**Response types** (`CoffeePeek.Shared.Kernel.Response`):
- `Response` — non-generic success/error, has `IsSuccess`, `Message`, `Data?`
- `Response<TData>` — generic, typed `Data` property
- `CreateEntityResponse` — for POST create operations, adds `EntityId`
- `UpdateEntityResponse<T>` — for PATCH update operations

**Usage:**
```csharp
return Response<LoginResponse>.Success(new LoginResponse(result.AccessToken, result.RefreshToken));
return UpdateEntityResponse<string>.Success(user.About, "Profile updated successfully");
```

**Handler return choices:**
- Return typed response object directly — Wolverine serializes it
- Return tuple `(TResponse, TEvent)` to simultaneously return data and publish a domain event

## Logging

**Framework:** Serilog via `CoffeePeek.Shared.Web.Logging.SerilogExtensions`

**Configuration:**
- Minimum level: `Information`
- Console sink with template: `[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}`
- Dev: `AnsiConsoleTheme.Code` (colored); Prod: no theme
- Read from `appsettings.json` via `ReadFrom.Configuration`
- Enriched with `FromLogContext()`

**Usage in infrastructure:** Use `ILogger<T>` injection — `ILogger<GlobalExceptionHandler> logger`

**Request logging:** `app.UseSerilogRequestLogging()` in `UseApplication()`

**Structured logging pattern:**
```csharp
logger.LogError(
    exception,
    "Exception occurred: {Message}. TraceId: {TraceId}",
    exception.Message, traceId);
```

## Domain Object Design

**Value Objects are `record` types with private constructor + static `Create` factory:**
```csharp
public partial record Email
{
    public string Value { get; init; }
    private Email(string value) { /* validate */ }
    public static Email Create(string value) => new(value);
    public static implicit operator string(Email email) => email.Value;
    public override string ToString() => Value;
}
```

**Aggregate Roots inherit `AggregateRoot<TId>`** which provides:
- `Id` property
- `CreatedAtUtc` / `UpdatedAtUtc` audit timestamps
- `AddDomainEvent()` / `GetDomainEvents()` for domain events
- Private parameterless constructor for EF Core — `private User() { }`

**Entity mutation is always via domain methods** — never set properties directly:
```csharp
user.UpdateAbout(command.About);
user.AssignRole(role);
user.SetSoftDelete();
```

## EF Core Configuration

**Owned entities** using `OwnsOne` — `UserCredential` and `UserStatistics` are owned by `User`

**Value object conversions:**
```csharp
builder.Property(u => u.Username)
    .HasConversion(v => v.Value, v => Username.Create(v))
    .HasMaxLength(BusinessConstants.MaxUserNameLength)
    .IsRequired();
```

**Constraint constants live in `BusinessConstants`** — reference them in EF config and domain validation alike

**Decorator pattern for caching:**
- `CachedUserRepository` wraps `UserRepository` and invalidates Redis on writes
- Registered in DI manually: `services.AddScoped<IUserRepository, UserRepository>()` (the caching variant is registered separately when needed)

## Comments

**When to Comment:**
- XML doc comments (`///`) on public controller actions, response types, and shared library public APIs
- Inline comments for non-obvious business logic — race conditions, security considerations, regression notes
- `// ReSharper disable once` suppressions where EF Core requires private constructors

**XML Doc pattern:**
```csharp
/// <summary>
/// Get user profile by id or current user profile
/// </summary>
```

**Inline regression comment example:**
```csharp
// C-1 regression: must be AddMinutes, not AddHours
```

## Module Registration

Each layer has its own `DependencyInjection.cs` with an extension method on `IServiceCollection`:
```csharp
services.AddApplication()     // CoffeePeek.Account.Application/DependencyInjection.cs
services.AddInfrastructure()  // CoffeePeek.Account.Infrastructure
services.AddPersistence(builder) // CoffeePeek.Account.Persistence/DependencyInjection.cs
services.AddPresentation()    // CoffeePeek.AccountService/DependencyInjection.cs
```

All wired together in `CoffeePeek.AccountService/InfrastructureExtensions.cs` → `AddApplication()`.

---

*Convention analysis: 2026-05-17*
