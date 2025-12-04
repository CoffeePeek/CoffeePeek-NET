# CoffeePeek.Data

Библиотека для работы с Entity Framework Core, предоставляющая базовые абстракции и реализации для работы с базой данных.

## Возможности

- **GenericRepository** - универсальный репозиторий для работы с сущностями
- **Unit of Work** - паттерн для управления транзакциями
- **Расширения для DI** - удобная регистрация сервисов

## Установка

Добавьте ссылку на проект в ваш `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\CoffeePeek.Data\CoffeePeek.Data.csproj" />
</ItemGroup>
```

## Использование

### 1. Регистрация DbContext

```csharp
using CoffeePeek.Data.Extensions;
using CoffeePeek.Shared.Extensions.Options;

var dbOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();
builder.Services.AddEfCoreData<YourDbContext>(dbOptions);
```

Или с прямой строкой подключения:

```csharp
builder.Services.AddEfCoreData<YourDbContext>("Host=localhost;Database=mydb;...");
```

### 2. Использование GenericRepository

#### Регистрация репозитория для конкретной сущности:

```csharp
builder.Services.AddGenericRepository<User, YourDbContext>();
```

#### Использование в сервисе:

```csharp
public class UserService
{
    private readonly IGenericRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IGenericRepository<User> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<User> CreateUserAsync(string email)
    {
        var user = new User { Email = email };
        await _repository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<User>> GetUsersByEmailAsync(string email)
    {
        return await _repository.FindAsync(u => u.Email == email);
    }

    public async Task UpdateUserAsync(User user)
    {
        _repository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### 3. Использование Unit of Work для транзакций

```csharp
public async Task TransferDataAsync()
{
    await _unitOfWork.BeginTransactionAsync();
    try
    {
        await _userRepository.AddAsync(user);
        await _orderRepository.AddAsync(order);
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### 4. Использование Query методов

```csharp
// С отслеживанием изменений
var users = _repository.Query()
    .Where(u => u.IsActive)
    .OrderBy(u => u.Email)
    .ToListAsync();

// Без отслеживания изменений (для чтения)
var users = _repository.QueryAsNoTracking()
    .Where(u => u.IsActive)
    .ToListAsync();
```

## API Reference

### IGenericRepository<TEntity>

- `GetByIdAsync<TKey>(TKey id)` - получить сущность по ID
- `GetAllAsync()` - получить все сущности
- `FindAsync(Expression<Func<TEntity, bool>> predicate)` - найти сущности по условию
- `FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)` - получить первую сущность или null
- `AnyAsync(Expression<Func<TEntity, bool>> predicate)` - проверить существование
- `CountAsync(Expression<Func<TEntity, bool>>? predicate)` - подсчитать количество
- `AddAsync(TEntity entity)` - добавить сущность
- `AddRangeAsync(IEnumerable<TEntity> entities)` - добавить несколько сущностей
- `Update(TEntity entity)` - обновить сущность
- `UpdateRange(IEnumerable<TEntity> entities)` - обновить несколько сущностей
- `Remove(TEntity entity)` - удалить сущность
- `RemoveRange(IEnumerable<TEntity> entities)` - удалить несколько сущностей
- `Query()` - получить IQueryable с отслеживанием
- `QueryAsNoTracking()` - получить IQueryable без отслеживания

### IUnitOfWork

- `SaveChangesAsync()` - сохранить изменения
- `BeginTransactionAsync()` - начать транзакцию
- `CommitTransactionAsync()` - зафиксировать транзакцию
- `RollbackTransactionAsync()` - откатить транзакцию


