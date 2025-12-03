# Переменные окружения для Railway

## База данных (PostgreSQL)

### AuthService и UserService - интеграция с Railway PostgreSQL

Сервисы автоматически используют переменные окружения, которые Railway предоставляет при подключении PostgreSQL сервиса.

### Поддерживаемые форматы переменных окружения:

#### 1. DATABASE_URL (рекомендуется)
Railway автоматически предоставляет эту переменную при подключении PostgreSQL:
```
DATABASE_URL=postgresql://user:password@host:port/database
```

#### 2. Отдельные переменные PostgreSQL
```
PGHOST=your-host
PGPORT=5432
PGDATABASE=railway
PGUSER=postgres
PGPASSWORD=your-password
```

#### 3. Прямая переменная окружения (альтернатива)
```
PostgresCpOptions__ConnectionString=Host=host;Port=5432;Database=railway;Username=postgres;Password=password;SslMode=Prefer;TrustServerCertificate=true;
```

### Приоритет переменных окружения:

1. `DATABASE_URL` (автоматически преобразуется в строку подключения .NET)
2. `PGHOST`, `PGPORT`, `PGDATABASE`, `PGUSER`, `PGPASSWORD` (автоматически объединяются)
3. `PostgresCpOptions__ConnectionString` (используется напрямую)
4. Значение из `appsettings.json` (fallback, если переменные не заданы)

---

## Redis

### Сервисы, использующие Redis: AuthService, Api

Сервисы автоматически используют переменные окружения Railway для подключения к Redis.

### Поддерживаемые форматы переменных окружения:

#### 1. REDIS_URL (рекомендуется)
Railway автоматически предоставляет эту переменную при подключении Redis:
```
REDIS_URL=redis://:password@host:port
```

#### 2. Отдельные переменные Redis
```
REDIS_HOST=your-host
REDIS_PORT=6379
REDIS_PASSWORD=your-password
```

#### 3. Прямые переменные окружения (альтернатива)
```
RedisOptions__Host=your-host
RedisOptions__Port=6379
RedisOptions__Password=your-password
```

### Приоритет переменных окружения:

1. `REDIS_URL` (автоматически преобразуется в RedisOptions)
2. `REDIS_HOST`, `REDIS_PORT`, `REDIS_PASSWORD` (автоматически объединяются)
3. `RedisOptions__Host`, `RedisOptions__Port`, `RedisOptions__Password` (используются напрямую)
4. Значение из `appsettings.json` (fallback, если переменные не заданы)

---

## RabbitMQ

### Сервисы, использующие RabbitMQ: AuthService, UserService, Api, Photo.Api, OutboxBackgroundService

Сервисы автоматически используют переменные окружения Railway для подключения к RabbitMQ.

### Поддерживаемые форматы переменных окружения:

#### 1. RABBITMQ_URL (рекомендуется)
Railway автоматически предоставляет эту переменную при подключении RabbitMQ:
```
RABBITMQ_URL=amqp://user:password@host:port
```

#### 2. Отдельные переменные RabbitMQ
```
RABBITMQ_HOST=your-host
RABBITMQ_PORT=5672
RABBITMQ_USER=your-user
RABBITMQ_PASSWORD=your-password
```

#### 3. Прямые переменные окружения (альтернатива)
```
RabbitMqOptions__HostName=your-host
RabbitMqOptions__Port=5672
RabbitMqOptions__Username=your-user
RabbitMqOptions__Password=your-password
```

### Приоритет переменных окружения:

1. `RABBITMQ_URL` (автоматически преобразуется в RabbitMqOptions)
2. `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_USER`, `RABBITMQ_PASSWORD` (автоматически объединяются)
3. `RabbitMqOptions__HostName`, `RabbitMqOptions__Port`, `RabbitMqOptions__Username`, `RabbitMqOptions__Password` (используются напрямую)
4. Значение из `appsettings.json` (fallback, если переменные не заданы)

---

## Как подключить сервисы в Railway:

1. В Railway UI откройте ваш проект
2. Добавьте нужные сервисы (PostgreSQL, Redis, RabbitMQ)
3. В настройках вашего сервиса перейдите в раздел **Variables**
4. Railway автоматически добавит переменные `DATABASE_URL`, `REDIS_URL`, `RABBITMQ_URL`
5. Или подключите сервисы через раздел **Networking**

### Примечание:

- Railway автоматически предоставляет переменные окружения при подключении сервисов
- Не нужно вручную добавлять переменные, если сервисы подключены через Railway Networking
- Если переменные окружения не заданы, будет использоваться значение из `appsettings.json`

