# Phase 05: Test Coverage — Research

**Researched:** 2026-05-20
**Domain:** .NET 10 / xUnit v3 / Moq / FluentAssertions — unit-тесты для Shops Application и Shops Domain
**Confidence:** HIGH

---

## Summary

Фаза закрывает критические пробелы в тестовом покрытии: 6 хэндлеров Shops Application, доменная модель Shops Domain, фикс бага TEST-04 (проглоченный `DomainException` в `CreateCheckInHandler`), и один хэндлер Account Application (`UpdateEmailRequestHandler`).

**Ключевой вывод по TEST-05 (deferred from Phase 2):** Тест `LoginAsync_WithValidCredentials_RevokesAllExistingSessions` в текущем коде репозитория **не существует** — он уже был переписан в `LoginAsync_WithValidCredentials_RevokesOldestSessionWhenAtLimit` и находится в `AuthServiceTests.cs`. Все 141 теста Account Application Tests зелёные (`dotnet test` подтверждает). Задача TEST-05 из deferred-items **уже выполнена** — нужно только верифицировать это в RESEARCH.md и не включать в план новую работу.

**Ключевой вывод по TEST-04:** `CreateCheckInHandler.cs` строка 74 содержит `catch (DomainException) { /* ignore */ }`. Это позволяет `Review.Create` с невалидным рейтингом молча упасть без уведомления. Для TEST-04 требуется: (1) исправить продакшн-код — заменить тихий `catch` на логирование/выброс; (2) написать тест, который проверяет, что хэндлер пробрасывает `DomainException` при невалидном рейтинге.

**Primary recommendation:** Организовать работу в 3 параллельных волны по изолированности зависимостей — Shops Domain Tests и Account Application Test (TEST-05 верификация) могут идти в Wave 1 одновременно с простыми Application хэндлерами; сложные хэндлеры с кэшем (Search, GetCoffeeShop) — в Wave 2; TEST-04 prodfix + тест — в Wave 3 как финальный.

---

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Shops Domain tests (Review, CheckIn invariants) | `CoffeePeek.Shops.Domain.Tests` | — | Домен не зависит от Application |
| Shops Application handler tests | `CoffeePeek.Shops.Application.Tests` | — | Тесты хэндлеров через прямой вызов статических методов |
| Account Application handler test (UpdateEmail) | `CoffeePeek.Account.Application.Tests` | — | Существующий тест-проект, паттерн уже установлен |
| TEST-04 production fix | `CoffeePeek.Shops.Application` | — | Исправление в самом хэндлере |

---

## Состояние тест-проектов (VERIFIED через чтение кода + dotnet test)

### `CoffeePeek.Shops.Application.Tests`

- **Существует:** да [VERIFIED: прочитан .csproj]
- **Ссылки:** `CoffeePeek.Shops.Application` (транзитивно тянет `Shops.Domain`, `Shared.Validation`, `Contract`)
- **Пакеты:** `xunit.v3`, `FluentAssertions`, `Moq`, `JetBrains.Annotations`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`
- **`GlobalUsings.cs`:** только `global using Xunit;` — все остальные using нужно добавлять явно в каждом файле
- **Существующие тесты:** `DeleteReviewFromCoffeeShopHandlerTests` (3 теста) + `CreateShopFromModerationServiceTests` (2 теста) = **5 тестов, все зелёные**
- **Ссылается на Shops.Domain напрямую?** Нет — только транзитивно через Application. Достаточно для создания доменных объектов (Review.Create и т.д.) в тестах хэндлеров.

### `CoffeePeek.Shops.Domain.Tests`

- **Существует:** да [VERIFIED: прочитан .csproj]
- **Ссылки:** `CoffeePeek.Shops.Domain` (прямая)
- **Пакеты:** `xunit.v3`, `FluentAssertions`, `JetBrains.Annotations`, `coverlet.collector`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`; **Moq отсутствует** (домен без зависимостей — не нужен)
- **`ImplicitUsings: enable`**, `Nullable: enable`
- **Существующие тесты (12):** `EquipmentTest` (8 тестов) + `CoffeeBeanCacheKeyTests` (1 тест) = **12 тестов, все зелёные**
- **Нет тестов на:** `Review.Create`, `Rating.Create`, `CheckIn.Create`, `CoffeeShop` конструктор

### `CoffeePeek.Account.Application.Tests`

- **Существует:** да [VERIFIED]
- **Пакеты:** `xunit.v3`, `FluentAssertions`, `Moq`, `JetBrains.Annotations`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`
- **`GlobalUsings.cs`:** `global using DomainUser = CoffeePeek.Account.Domain.Entities.UserAggregate.User;`
- **Существующие тесты (141):** AuthService (7), Login (несколько), RefreshToken, Register, Logout, ConfirmEmail, ResendEmail, OAuth, UpdateAbout, UpdateAvatar, UpdatePhone, UpdateUsername, DeleteUser — **все 141 зелёные**
- **Нет тестов на:** `UpdateEmailRequestHandler`

---

## TEST-05: AuthServiceTests — Статус ВЫПОЛНЕНО

**Факт:** Тест `LoginAsync_WithValidCredentials_RevokesAllExistingSessions` **не существует** в кодовой базе. Вместо него уже присутствуют корректные тесты:

- `LoginAsync_WithValidCredentials_RevokesOldestSessionWhenAtLimit` — проверяет что при достижении `MaxActiveSessions` старейшая сессия отзывается
- `LoginAsync_WithValidCredentials_KeepsExistingSessionsWhenUnderLimit` — проверяет что под лимитом сессии сохраняются
- `LoginAsync_WithValidCredentials_CreatesNewSession` — проверяет что новый токен добавляется

`dotnet test` → **141 passed, 0 failed**. [VERIFIED: dotnet test run]

**Вывод для плановика:** Никакой работы по AuthServiceTests не требуется. Задача из deferred-items закрыта. В план включать только фиксацию этого факта.

---

## Handler-by-Handler Breakdown (TEST-01)

### 1. `SearchCoffeeShopsHandler`

**Файл:** `CoffeePeek.Shops.Application/Features/CoffeeShop/SearchCoffeeShops/SearchCoffeeShopsHandler.cs`
**Класс:** `public class SearchCoffeeShopsHandler` (не static) со static методом `Handle`

**Зависимости для мока:**
```csharp
Mock<ICoffeeShopQueries> _shopQueriesMock
Mock<IUserFavoriteRepository> _favoriteRepoMock
Mock<IQueryCheckInRepository> _visitRepoMock
Mock<ICacheService> _cacheMock    // из CoffeePeek.Shared.Domain.Interfaces.Infrastructure
```

**Специфика ICacheService в тесте:** Хэндлер использует перегрузку `GetAsync<T>(cacheKey, factory, cancellationToken)`. При моке нужно настроить `_cacheMock.Setup(c => c.GetAsync(It.IsAny<CacheKey>(), It.IsAny<Func<CancellationToken, Task<GetCoffeeShopsResponse>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse)`. Альтернатива — поднять реальный `InMemoryCacheService` stub, но это усложняет тест. Простой путь: мокировать `GetAsync` с factory через `ReturnsAsync`.

**Happy paths:**
- Анонимный запрос (`UserId == null`) → возвращает кэшированный ответ, не вызывает `favoriteRepository` и `visitRepository`
- Авторизованный запрос (`UserId.HasValue`) → вызывает `GetFavoriteShopIdsAsync` и `GetVisitedShopIdsAsync`, обогащает `IsFavorite`/`IsVisited`
- Результат = `Response<GetCoffeeShopsResponse>.IsSuccess == true`

**Error paths:**
- `ICacheService.GetAsync` вернул `null` → хэндлер возвращает `Response.Error("Failed to retrieve...")`

**Namespace:** `CoffeePeek.Shops.Application.Tests.Features.CoffeeShop.SearchCoffeeShops`

---

### 2. `GetCoffeeShopHandler`

**Файл:** `CoffeePeek.Shops.Application/Features/CoffeeShop/GetCoffeeShop/GetCoffeeShopHandler.cs`
**Класс:** `public static class GetCoffeeShopHandler`

**Зависимости для мока:**
```csharp
Mock<ICoffeeShopQueries> _shopQueriesMock
Mock<IUserFavoriteRepository> _favoriteRepoMock
Mock<IQueryCheckInRepository> _checkInRepoMock
Mock<IQueryReviewRepository> _reviewRepoMock
Mock<ICacheService> _cacheMock
Mock<IMapper> _mapperMock   // Mapster IMapper
```

**ICacheService mock:** Аналогично Search — factory перегрузка, но с `TimeSpan? expiration` параметром.

**Happy paths:**
- Анонимный запрос → возвращает shopDto без IsFavorite/IsVisited обогащения
- Авторизованный запрос → вызывает `favoriteRepository.Exists` и `checkInRepository.Exists`

**Error paths:**
- Кэш/запрос вернул `null` → `Response.Error("Shop not found")`

**Mapper:** При создании `CoffeeShopDetailsDto with { Reviews = mapper.Map<ReviewDto[]>(reviews) }` — нужно мокировать `_mapperMock.Setup(m => m.Map<ReviewDto[]>(It.IsAny<IReadOnlyList<Review>>())).Returns([])`.

---

### 3. `CreateCheckInHandler` (TEST-01 + TEST-04)

**Файл:** `CoffeePeek.Shops.Application/Features/CheckIn/CreateCheckIn/CreateCheckInHandler.cs`
**Класс:** `public static class CreateCheckInHandler`

**Зависимости для мока:**
```csharp
Mock<IQueryCheckInRepository> _checkInRepoMock
Mock<IUnitOfWork> _unitOfWorkMock
Mock<IMessageBus> _busMock   // из Wolverine
Mock<IAsyncValidationStrategy<CreateCheckInCommand>> _validationMock
Mock<IMapper> _mapperMock
```

**Happy paths:**
- Непубличный check-in (`IsPublic = false`) → создаёт CheckIn, не вызывает `Review.Create`, вызывает `SaveChangesAsync`
- Публичный check-in с валидным рейтингом → создаёт CheckIn, создаёт Review, публикует `CheckinCreatedEvent`, вызывает `SaveChangesAsync`

**TEST-04 — Баг и фикс:**

**Текущий баг (строка 74):**
```csharp
catch (DomainException) { /* ignore */ }
```
Это молча проглатывает `DomainException` из `Review.Create` при невалидном рейтинге (например, Place=0 нарушает `MinReviewRate=1`). Пользователь получает успешный ответ, но отзыв не создан и событие не опубликовано.

**Требуемый фикс в продакшн-коде:**
```csharp
// Строка 74: ЗАМЕНИТЬ
catch (DomainException) { /* ignore */ }

// НА:
catch (DomainException)
{
    throw;   // или: throw new ValidationException("Invalid review data: " + ex.Message, ex);
}
```

Более точный вариант: публичный check-in требует валидный рейтинг — `DomainException` здесь означает дефект данных от клиента, поэтому правильно пробрасывать его (или оборачивать в `ValidationException`). Выбор — пробросить `DomainException` (проще, тест проверяет конкретный тип).

**TEST-04 Test (после фикса):**
```csharp
[Fact]
public async Task Handle_PublicCheckIn_WithInvalidRating_ThrowsDomainException()
{
    // rating.Place = 0 < MinReviewRate(1) → Review.Create throws DomainException
    var command = new CreateCheckInCommand(
        CoffeeShopId: Guid.NewGuid(),
        IsPublic: true,
        VisitedAt: DateTime.UtcNow.AddHours(-1),
        Note: "Some valid note here",
        Photos: null,
        Rating: new RatingDto { Place = 0, Service = 3, Coffee = 3 }
    ) { UserId = Guid.NewGuid(), UserName = "testuser" };

    _validationMock.Setup(v => v.ValidateAsync(command, _ct))
        .ReturnsAsync(Shared.Validation.ValidationResult.Valid);

    var act = async () => await CreateCheckInHandler.Handle(
        command, _checkInRepoMock.Object, _unitOfWorkMock.Object,
        _busMock.Object, _validationMock.Object, _mapperMock.Object, _ct);

    await act.Should().ThrowAsync<DomainException>();
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}
```

**Важно:** `CheckInValidationStrategy` допускает рейтинг 0-5 (`Rating.Coffee < 0 || Rating.Coffee > 5`), но `Rating.Create` в домене требует 1-5 (`MinReviewRate = 1`). Это несоответствие — рейтинг `Place=0` пройдёт application-level валидацию, но упадёт в `Review.Create`. После фикса catch-блока этот `DomainException` будет пробрасываться.

---

### 4. `AddToFavoriteHandler`

**Файл:** `CoffeePeek.Shops.Application/Features/Favorite/AddToFavorite/AddToFavoriteHandler.cs`
**Класс:** `public class AddToFavoriteHandler` (не static) со static методом `Handle`

**Зависимости для мока:**
```csharp
Mock<IUserFavoriteService> _favoriteServiceMock
Mock<IValidationStrategy<AddToFavoriteCommand>> _validationMock
Mock<IUnitOfWork> _unitOfWorkMock
```

**Happy paths:**
- Валидная команда → вызывает `AddToFavoritesAsync`, затем `SaveChangesAsync`, возвращает `CreateEntityResponse<Guid>.IsSuccess == true`

**Error paths:**
- `CoffeeShopId == Guid.Empty` → `ValidationException("CoffeeShopId is required and cannot be empty")`
- `UserId == Guid.Empty` → `ValidationException("UserId is required and cannot be empty")`

**Примечание:** `AddToFavoriteValidationStrategy` — конкретная реализация, не нужно использовать реальный класс в тестах. Можно мокировать `IValidationStrategy<AddToFavoriteCommand>` или использовать реальный `AddToFavoriteValidationStrategy` без мока (он не имеет зависимостей).

---

### 5. `DeleteReviewFromCoffeeShopHandler`

**Статус: DONE (TEST-03)** — 3 теста уже существуют в `CoffeePeek.Shops.Application.Tests/Features/Review/DeleteReviewFromCoffeeShopHandlerTests.cs`:
- `Handle_WhenOwnerDeletesOwnReview_SoftDeletesAndSaves`
- `Handle_WhenNonOwnerAttemptsDelete_ThrowsForbiddenExceptionAndDoesNotSave`
- `Handle_WhenReviewNotFound_ThrowsNotFoundException`

Никакой дополнительной работы не требуется.

---

### 6. `GetShopsInBoundsHandler`

**Файл:** `CoffeePeek.Shops.Application/Features/CoffeeShop/GetShopsInBounds/GetShopsInBoundsHandler.cs`
**Класс:** `public class GetShopsInBoundsHandler` со static методом `Handle`

**Зависимости для мока:**
```csharp
Mock<ICoffeeShopQueries> _shopQueriesMock
```

Самый простой хэндлер — нет кэша, нет пользовательского контекста.

**Happy paths:**
- Вызывает `shopQueries.GetShopsInBounds(query, ct)`, оборачивает в `GetShopsInBoundsResponse`, возвращает `Response.Success`
- Пустой массив (нет магазинов) → возвращает `Response.Success` с пустым массивом

---

## TEST-02: Shops Domain Unit Tests

### `Review.Create` Validation

**Файл тестов:** `CoffeePeek.Shops.Domain.Tests/Aggregates/ReviewAggregate/ReviewTests.cs` (новый)

Все инварианты из `ReviewAction.cs`:

| Сценарий | Ожидание |
|----------|---------|
| `shopId == Guid.Empty` | `DomainException("CoffeeShopId cannot be empty.")` |
| `userId == Guid.Empty` | `DomainException("UserId cannot be empty.")` |
| `header` пустой/whitespace | `DomainException("Review header is required.")` |
| `header.Length < 3` (MinReviewHeaderLength) | `DomainException("header must be between 3 and 100 characters.")` |
| `header.Length > 100` (MaxReviewHeaderLength) | `DomainException("header must be between 3 and 100 characters.")` |
| `comment` пустой/whitespace | `DomainException("Review comment is required.")` |
| `comment.Length < 10` (MinReviewCommentLength) | `DomainException("comment must be between 10 and 1000 characters.")` |
| `comment.Length > 1000` (MaxReviewCommentLength) | `DomainException("comment must be between 10 and 1000 characters.")` |
| Все валидно | возвращает `Review` с заполненными полями |

### `Rating.Create` Validation

**Файл тестов:** `CoffeePeek.Shops.Domain.Tests/Entities/RatingTests.cs` (новый)

Инварианты из `Rating.cs`:

| Сценарий | Ожидание |
|----------|---------|
| `coffee < 1` (MinReviewRate) | `DomainException("coffee must be between 1 and 5.")` |
| `coffee > 5` (MaxReviewRate) | `DomainException(...)` |
| `place < 1` | `DomainException("place must be between 1 and 5.")` |
| `place > 5` | `DomainException(...)` |
| `service < 1` | `DomainException("service must be between 1 and 5.")` |
| `service > 5` | `DomainException(...)` |
| Все в диапазоне 1-5 | возвращает `Rating`, `AverageRating = (place + place + service) / 3m` |

**Замечание о баге в `Rating`:** `AverageRating = (Place + Place + Service) / 3m` — здесь дважды `Place` вместо `Place + Coffee + Service`. Это существующий баг (не в scope Phase 5 исправлять, но тест должен документировать текущее поведение, а не ожидаемое корректное).

### `CheckIn.Create` Invariants

**Файл тестов:** `CoffeePeek.Shops.Domain.Tests/Aggregates/CheckInAggregate/CheckInTests.cs` (новый)

Инварианты из `CheckInAction.cs`:

| Сценарий | Ожидание |
|----------|---------|
| `userId == Guid.Empty` | `DomainException("UserId is required.")` |
| `shopId == Guid.Empty` | `DomainException("ShopId is required.")` |
| Оба валидны | возвращает `CheckIn` с заполненными `UserId`, `ShopId`, `VisitedAt` |
| `UpdateNote(null)` | `Note == null` |
| `UpdateNote("  trim  ")` | `Note == "trim"` |

### `CoffeeShop` Constructor Tests

**Файл тестов:** `CoffeePeek.Shops.Domain.Tests/Aggregates/CoffeeShopAggregate/CoffeeShopTests.cs` (новый)

`CoffeeShop` не имеет публичного static factory с валидацией — конструктор принимает параметры без DomainException (валидация не реализована на уровне домена). Тесты для конструктора:

| Сценарий | Ожидание |
|----------|---------|
| Валидные данные | `Id != Guid.Empty`, `Name == name`, `CreatorId == creatorId`, `Status == Active` |
| `IsNew` для нового магазина | `true` (CreatedAtUtc дефолтный — нет, CreatedAtUtc установлен через Entity базовый класс... нужно проверить) |
| `IsOpen` без расписания | `true` (статус Active, нет расписания) |
| `IsOpen` с закрытым статусом | `false` |
| `AddEquipment` — дубликат бренда+модели | не добавляется дважды |

**Важно:** `CreatedAtUtc` в Entity базовом классе — нужно проверить, устанавливается ли он в конструкторе. [ASSUMED: вероятно, инициализируется как `DateTime.UtcNow` или остаётся `DateTime.MinValue` без явной инициализации в `CoffeeShop()` конструкторе — `IsNew` может вести себя неожиданно в тестах]

---

## TEST-05: `UpdateProfileEmailHandler` (Account Application)

**Класс:** `UpdateEmailRequestHandler` (не `UpdateProfileEmailHandler`!) в `CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateEmail`

**Файл тестов:** `CoffeePeek.Account.Application.Tests/Features/User/UpdateUserProfile/UpdateEmail/UpdateEmailRequestHandlerTest.cs` (новый)

**Зависимости для мока:**
```csharp
Mock<IUserRepository> _userRepositoryMock
Mock<IUnitOfWork> _unitOfWorkMock
```

**Паттерн создания User (из существующих тестов):**
```csharp
var role = Role.Create("User");
var user = DomainUser.Register("old@example.com", "testuser", "hash", role);
// Подтвердить email (для полной корректности не обязательно для этого хэндлера):
typeof(UserCredential)
    .GetProperty(nameof(UserCredential.EmailConfirmed))!
    .SetValue(user.Credentials, true);
```

**Happy path:**
- Пользователь найден, email не занят другим пользователем → обновляет email, вызывает `userRepository.Update`, `unitOfWork.SaveChangesAsync`, возвращает кортеж `(UpdateEntityResponse<string>, UserRegisteredInternalEvent)`
- `response.IsSuccess == true`
- `event.Email == newEmail`, `event.UserId == userId`, `event.ConfirmationToken != null`

**Error paths:**
- `userRepository.GetById` возвращает `null` → `NotFoundException("User not found")`
- Email уже занят другим пользователем (`existingOwner.Id != request.UserId`) → `DomainException("Email is already taken")`
- Email принадлежит тому же пользователю (`existingOwner.Id == request.UserId`) → успешно обновляет (идемпотентность)

**Тонкость:** `user.Credentials.UpdateEmail(request.Email)` вызывает `ResetEmailConfirmedFlow()` — устанавливает `EmailConfirmed = false` и генерирует новый `EmailConfirmationToken`. Тест должен проверить `event.ConfirmationToken != null`.

---

## Производственный фикс (TEST-04): Точная инструкция

**Файл:** `CoffeePeek.Shops.Application/Features/CheckIn/CreateCheckIn/CreateCheckInHandler.cs`
**Строка:** 74 (подтверждено grep)

**Текущий код (строки 72-74):**
```csharp
            }
            catch (DomainException) { /* ignore */ }
        }
```

**Замена на:**
```csharp
            }
            catch (DomainException)
            {
                throw;
            }
        }
```

**Обоснование:** Публичный check-in требует валидного рейтинга. Если `Review.Create` бросает `DomainException` при невалидном рейтинге (Place=0, например) — это ошибка данных на стороне клиента. Молчать нельзя: пользователь думает, что check-in создан с отзывом, хотя отзыв не создан и событие не опубликовано. Пробрасывание `DomainException` приведёт к HTTP 422 через `GlobalExceptionHandler`, что корректно.

**Дополнительно:** После фикса нужно согласовать `CheckInValidationStrategy` — она допускает рейтинг 0 (`Rating.Coffee < 0`), а домен — нет (`MinReviewRate = 1`). Рекомендация: обновить условие в `CheckInValidationStrategy` с `< 0` на `< 1` для консистентности. Это отдельная задача в том же плане.

---

## Wave Grouping Recommendation

### Wave 1 — Параллельно (нет взаимозависимостей)

**Plan 05-01: Shops Domain Tests**
- `CoffeePeek.Shops.Domain.Tests` — добавить тесты для `Review.Create`, `Rating.Create`, `CheckIn.Create`, `CoffeeShop` constructor
- Нет изменений в продакшн-коде
- Быстро: чистая домен-логика, нет моков

**Plan 05-02: AuthServiceTests — Verification + UpdateEmailRequestHandler**
- Верифицировать что AuthServiceTests зелёные (TEST-05 deferred — already done)
- Добавить `UpdateEmailRequestHandlerTest.cs` в `CoffeePeek.Account.Application.Tests`
- Нет изменений в продакшн-коде

### Wave 2 — После Wave 1 (простые Application хэндлеры)

**Plan 05-03: Simple Application Handlers**
- `AddToFavoriteHandler` tests — простой, нет кэша
- `GetShopsInBoundsHandler` tests — самый простой
- `DeleteReviewFromCoffeeShop` — SKIP (уже готово)

### Wave 3 — После Wave 2 (комплексные с кэшем + продакшн фикс)

**Plan 05-04: TEST-04 Production Fix + CreateCheckInHandler Tests**
- Исправить `catch (DomainException) { /* ignore */ }` → `throw`
- Согласовать `CheckInValidationStrategy` (< 0 → < 1)
- Написать тесты `CreateCheckInHandler`

**Plan 05-05: Cache-Heavy Handlers**
- `SearchCoffeeShopsHandler` tests (с `ICacheService` mock)
- `GetCoffeeShopHandler` tests (с `ICacheService` и `IMapper` mock)

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Мок ICacheService | Реальный Redis/InMemory | `Mock<ICacheService>` + `ReturnsAsync` | Unit тест не требует инфраструктуры |
| Мок IMapper | Реальный Mapster mapper | `Mock<IMapper>` + `.Returns([])` | Unit тест изолирует маппинг |
| Создание User в тестах | Reflection-heavy factory | `DomainUser.Register(...)` + рефлексия для подтверждения email (паттерн из существующих тестов) | Уже задокументировано в UpdateUsernameHandlerTest.cs |
| Проверка что SaveChanges вызван | Ручная проверка флагов | `_unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once)` | Moq Verify паттерн |

---

## Common Pitfalls

### Pitfall 1: ICacheService factory mock

**What goes wrong:** `SearchCoffeeShopsHandler` и `GetCoffeeShopHandler` вызывают `redisService.GetAsync(cacheKey, factory, expiration, ct)` — это перегрузка с async factory. При моке `It.IsAny<Func<CancellationToken, Task<T>>>()` может не матчиться если перепутать параметры.

**How to avoid:** Использовать `ReturnsAsync` напрямую через мок, не вызывая factory:
```csharp
_cacheMock
    .Setup(c => c.GetAsync(
        It.IsAny<CacheKey>(),
        It.IsAny<Func<CancellationToken, Task<GetCoffeeShopsResponse>>>(),
        It.IsAny<TimeSpan?>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(expectedResponse);
```

### Pitfall 2: Класс называется UpdateEmailRequestHandler, не UpdateProfileEmailHandler

**What goes wrong:** Задача говорит "тест для UpdateProfileEmailHandler", но в коде класс называется `UpdateEmailRequestHandler`. Namespace правильный: `...UpdateEmail`.

**How to avoid:** Использовать `UpdateEmailRequestHandler.Handle(...)` в тесте.

### Pitfall 3: GlobalUsings в Shops.Application.Tests содержит только `global using Xunit`

**What goes wrong:** В Account.Application.Tests есть `global using DomainUser = ...`. В Shops.Application.Tests — только Xunit. Все using нужно добавлять явно.

**How to avoid:** Копировать паттерн из `DeleteReviewFromCoffeeShopHandlerTests.cs` — там все using прописаны явно.

### Pitfall 4: IMessageBus из Wolverine требует NuGet-ссылки в тесте

**What goes wrong:** `CoffeePeek.Shops.Application.Tests.csproj` ссылается только на Application проект. Wolverine идёт транзитивно через Application. Убедиться что `IMessageBus` доступен в тестах без явной ссылки на WolverineFx.

**How to avoid:** Проверить что `Mock<IMessageBus>` компилируется — Wolverine тянется транзитивно через Application. Если нет — добавить `<PackageReference Include="WolverineFx" />` в тест-проект.

### Pitfall 5: Rating.AverageRating баг

**What goes wrong:** `AverageRating = (Place + Place + Service) / 3m` — дважды Place. Тест не должен проверять формульно-корректное значение, а должен проверять *текущее* поведение.

**How to avoid:** В тесте для `Rating.Create` не проверять `AverageRating` семантически, либо документировать баг комментарием.

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit v3 3.2.2 |
| Config file | нет (OutputType=Exe в .csproj) |
| Quick run command | `dotnet test CoffeePeek.Shops.Application.Tests` |
| Full suite command | `dotnet test CoffeePeek.slnx` |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| TEST-01 | SearchCoffeeShopsHandler — анонимный/авторизованный/null cache | unit | `dotnet test --filter "SearchCoffeeShopsHandlerTests"` | ❌ Wave 2 |
| TEST-01 | GetCoffeeShopHandler — анонимный/авторизованный/not found | unit | `dotnet test --filter "GetCoffeeShopHandlerTests"` | ❌ Wave 2 |
| TEST-01 | CreateCheckInHandler — public/private/validation | unit | `dotnet test --filter "CreateCheckInHandlerTests"` | ❌ Wave 3 |
| TEST-01 | AddToFavoriteHandler — success/empty CoffeeShopId/UserId | unit | `dotnet test --filter "AddToFavoriteHandlerTests"` | ❌ Wave 2 |
| TEST-01 | DeleteReviewFromCoffeeShop | unit | `dotnet test --filter "DeleteReviewFromCoffeeShopHandlerTests"` | ✅ Exists |
| TEST-01 | GetShopsInBoundsHandler — success/empty | unit | `dotnet test --filter "GetShopsInBoundsHandlerTests"` | ❌ Wave 2 |
| TEST-02 | Review.Create validation | unit | `dotnet test CoffeePeek.Shops.Domain.Tests` | ❌ Wave 1 |
| TEST-02 | Rating.Create validation | unit | `dotnet test CoffeePeek.Shops.Domain.Tests` | ❌ Wave 1 |
| TEST-02 | CheckIn.Create invariants | unit | `dotnet test CoffeePeek.Shops.Domain.Tests` | ❌ Wave 1 |
| TEST-02 | CoffeeShop constructor | unit | `dotnet test CoffeePeek.Shops.Domain.Tests` | ❌ Wave 1 |
| TEST-03 | DeleteReview returns 403 for non-owner | unit | `dotnet test --filter "DeleteReviewFromCoffeeShopHandlerTests"` | ✅ DONE |
| TEST-04 | CreateCheckIn не глотает DomainException | unit + prodfix | `dotnet test --filter "CreateCheckInHandlerTests"` | ❌ Wave 3 |
| TEST-05 | UpdateProfileEmailHandler | unit | `dotnet test CoffeePeek.Account.Application.Tests` | ❌ Wave 1 |
| Deferred | AuthServiceTests RevokesAllExistingSessions | — | — | ✅ Already done |

### Wave 0 Gaps
- Нет: существующая тест-инфраструктура полностью покрывает фазу. Новые файлы добавляются в уже настроенные проекты.

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET 10 SDK | Все тесты | ✓ | 10.0.100 | — |
| dotnet test | CI + локальный запуск | ✓ | встроен | — |
| WolverineFx (транзитивно) | Mock<IMessageBus> | ✓ | 5.30.0 через Application | — |
| Mapster (транзитивно) | Mock<IMapper> | ✓ | 10.0.7 через Application | — |

---

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | `CreatedAtUtc` в `CoffeeShop` инициализируется как минимум через базовый Entity конструктор | CoffeeShop tests | `IsNew` тест может давать неожиданные результаты — нужно проверить Entity базовый класс |
| A2 | `IMessageBus` доступен транзитивно в Shops.Application.Tests без явной PackageReference | CreateCheckInHandler tests | Если нет — добавить `<PackageReference Include="WolverineFx" />` в .csproj |

---

## Sources

### Primary (HIGH confidence)
- Прямое чтение исходного кода репозитория — все handler файлы, интерфейсы, доменные объекты
- `dotnet test CoffeePeek.slnx` — подтверждение текущего состояния тестов (400 total, 0 failed)
- Деплаза `deferred-items.md` из Phase 2 — описание задачи TEST-05

### Secondary (MEDIUM confidence)
- Существующие тест-файлы как паттерн — `DeleteReviewFromCoffeeShopHandlerTests.cs`, `UpdateUsernameHandlerTest.cs`, `RefreshTokenHandlerTests.cs`

---

## Metadata

**Confidence breakdown:**
- Существующий код (хэндлеры, интерфейсы, домен): HIGH — прочитан напрямую
- Паттерн тестирования: HIGH — взят из существующих тестов проекта
- TEST-05 статус: HIGH — подтверждён `dotnet test`
- TEST-04 фикс: HIGH — конкретная строка верифицирована grep
- Wave grouping: MEDIUM — разумная группировка по зависимостям, может быть скорректирована

**Research date:** 2026-05-20
**Valid until:** 2026-06-20 (стабильная кодовая база, нет быстро меняющихся зависимостей)
