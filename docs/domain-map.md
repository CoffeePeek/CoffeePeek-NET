# Архитектурная карта доменов (DDD Context Map)

Этот документ описывает границы контекстов (Bounded Contexts) и их взаимодействия в проекте CoffeePeek.

## Ограниченные контексты

### 1. Account Context (CoffeePeek.AccountService)
- **Ответственность**: Управление пользователями, аутентификация (JWT), профили.
- **Ключевые сущности**: User, Role, PhotoMetadata.
- **Интеграции**: Identity Provider — Выдает токены для всех остальных сервисов.
- **Подробная документация**: [Account Service Docs](account-service.md)

    #### Login (`/api/token` `POST`)
    Все сессии отзываются в базе, и после чего создаётся новая сессия с новым access и refresh token.
    
    ### Refresh (`/api/token` `PUT`)
    Создаётся новый refresh token, после чего происходит rotate. Если токен не был отозван, не истёк и совпадает с пользователем, то всё произоёт успешно. Создастся новая сессия и пользователю дадут новую пару токенов.

    ### Register (`/api/users` `POST`)
    Проверяется по паттерну Bloom Filter существование пользователя по email. Если пользователя нету, то создаётся новый с валидацией. И добавляется дефолтная роль User. После успешной регистрации создаётся ивент `UserRegisteredInternalEvent` который обрабатывается и отправляеться сообщение на почту с помощью `Resend`

    ### Check User Exists (`/api/users` `GET`)
    Проверяет существование сначала с Bloom Filter, а потом проверяет в БД существование такого email.

    ### Logout (`/api/tokens` `DELETE`)
    Закрывает текущую сессию пользователя, использует куки `refreshToken`.

    ### Get Profile (`/api/users/{id}` `GET`)
    Получение публичного профиля пользователя по его ID.

    ### Update Profile phone number(`/api/users/me/phone-number` `PATCH`)
    ### Update Profile about (`/api/users/me/about` `PATCH`)
    ### Update Profile username (`/api/users/me/username` `PATCH`)
    ### Update Profile Avatar (`/api/users/me/avatar` `PUT`)
    Обновление данных профиля текущего авторизованного пользователя.
    ### Update Profile Email (`/api/users/me/email` `PATCH`)
    Запрос на смену email. Требует последующего подтверждения.

    ### Email Confirmation (`/api/users/me/email-confirmation` `POST`/`PUT`)
    `POST` — Переотправка письма для подтверждения email.
    `PUT` — Подтверждение email с использованием токена из письма.

    ### Delete User (`/api/users/me/{id}` `DELETE`)
    Удаление аккаунта пользователя.

    ### OAuthLogin (`api/tokens/google/login` `POST`)
    - Не тестировалось, непонятна реализация
    
### 2. Shops Context (CoffeePeek.ShopsService)
- **Ответственность**: Каталог кофейных шопов, геопозиции, меню.
- **Ключевые сущности**: CoffeeShop, Menu, Location.

  #### Login (`/api/token` `POST`)
  Все сессии отзываются в базе, и после чего создаётся новая сессия с новым access и refresh token.

### 3. Job Vacancies Context (CoffeePeek.JobVacancies)
- **Ответственность**: Объявления о работе, отклики.
- **Ключевые сущности**: Vacancy, Application.
- **Связи**: Ссылается на CoffeeShop ID из Shops Context.

### 4. Moderation Context (CoffeePeek.ModerationService)
- **Ответственность**: Проверка контента (фото, отзывы).
- **Ключевые сущности**: ModerationItem, Report.

## Схема взаимодействий (Mermaid)

```mermaid
graph TD
    Gateway[API Gateway / YARP]
    
    Account[Account Service]
    Shops[Shops Service]
    Jobs[Job Vacancies Service]
    Moderation[Moderation Service]
    
    Gateway --> Account
    Gateway --> Shops
    Gateway --> Jobs
    Gateway --> Moderation
    
    Jobs -- "Reference ShopId" --> Shops
    Shops -- "Check Status" --> Moderation
    Moderation -- "Notify" --> Account
```

## Стратегия интеграции
- **Asynchronous**: RabbitMQ для событий (например, `UserCreated`, `ShopModerated`).
- **Synchronous**: HTTP/gRPC только в исключительных случаях через API Gateway или внутренние адреса.
