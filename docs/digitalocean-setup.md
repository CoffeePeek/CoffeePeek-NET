# Настройка DigitalOcean App Platform для CoffeePeek

## Предварительные требования

1. Аккаунт в DigitalOcean
2. Personal Access Token с правами на App Platform
3. GitHub репозиторий с настроенными секретами

## Шаг 1: Создание Personal Access Token

1. Перейдите в DigitalOcean Dashboard → API → Tokens/Keys
2. Создайте новый Personal Access Token
3. Дайте ему права на чтение и запись App Platform
4. Сохраните токен (он показывается только один раз!)

## Шаг 2: Создание App Platform приложения

### Вариант A: Через веб-интерфейс

1. Перейдите в DigitalOcean Dashboard → App Platform → Create App
2. Выберите "Docker Hub" как источник
3. Для каждого сервиса создайте компонент:
   - **Component Type**: Service
   - **Source**: Docker Hub
   - **Image**: `docker.io/<your-dockerhub-username>/coffeepeek.authservice:dev`
   - **HTTP Port**: 80
   - **Instance Size**: Basic (XXS) - можно изменить позже
   - **Instance Count**: 1

4. Повторите для всех сервисов:
   - `coffeepeek.authservice`
   - `coffeepeek.authservice.outbox-bg`
   - `coffeepeek.userservice`
   - `coffeepeek.shopsservice`
   - `coffeepeek.moderationservice`
   - `coffeepeek.jobvacancies`
   - `gateway`

### Вариант B: Через doctl CLI

```bash
# Установите doctl
# macOS: brew install doctl
# Linux: https://docs.digitalocean.com/reference/doctl/how-to/install/

# Авторизуйтесь
doctl auth init

# Создайте приложение из app.yaml
doctl apps create --spec .digitalocean/app.yaml
```

## Шаг 3: Настройка переменных окружения

Для каждого компонента настройте переменные окружения:

### Общие переменные для всех сервисов:
- `ASPNETCORE_URLS=http://+:80`
- `DOTNET_RUNNING_IN_CONTAINER=true`

### Специфичные переменные (пример для AuthService):
- `PostgresCpOptions__ConnectionString=<your-postgres-connection-string>`
- `RabbitMqOptions__HostName=<your-rabbitmq-host>`
- `RabbitMqOptions__Username=<your-rabbitmq-username>`
- `RabbitMqOptions__Password=<your-rabbitmq-password>`
- `JWTOptions__SecretKey=<your-jwt-secret>`
- И т.д.

## Шаг 4: Настройка GitHub Secrets

Добавьте следующие секреты в GitHub репозиторий (Settings → Secrets and variables → Actions):

1. `DIGITALOCEAN_ACCESS_TOKEN` - Personal Access Token из шага 1
2. `DIGITALOCEAN_APP_ID` - ID приложения (можно получить через `doctl apps list`)

## Шаг 5: Настройка маршрутизации

В App Platform настройте маршруты для Gateway:

- `/api/auth/*` → authservice
- `/api/user/*` → userservice
- `/api/shops/*` → shopsservice
- `/api/moderation/*` → moderationservice
- `/api/jobs/*` → jobvacancies
- `/` → gateway (основной маршрут)

## Шаг 6: Проверка деплоя

1. Сделайте push в ветку `main` или `dev`
2. GitHub Actions соберет образы и запустит job `deploy_digitalocean`
3. Проверьте логи в DigitalOcean Dashboard → App Platform → Deployments
4. Убедитесь, что все компоненты успешно задеплоились

## Управление через doctl

```bash
# Список приложений
doctl apps list

# Получить ID приложения
doctl apps list --format ID,Spec.Name

# Просмотр статуса деплоя
doctl apps get <app-id>

# Список компонентов
doctl apps list-components <app-id>

# Создать новый деплой
doctl apps create-deployment <app-id> --force-rebuild

# Просмотр логов
doctl apps logs <app-id> --component <component-name> --type run
```

## Мониторинг и масштабирование

1. **Мониторинг**: DigitalOcean предоставляет встроенный мониторинг в Dashboard
2. **Масштабирование**: Можно настроить автоподстройку или масштабировать вручную
3. **Логи**: Доступны через Dashboard или `doctl apps logs`

## Стоимость

- Basic XXS: ~$5/мес за компонент
- Basic XS: ~$12/мес за компонент
- Для 7 сервисов на Basic XXS: ~$35/мес

## Troubleshooting

### Проблема: Компонент не обновляется
- Проверьте, что `DIGITALOCEAN_APP_ID` правильный
- Убедитесь, что токен имеет права на запись
- Проверьте логи GitHub Actions

### Проблема: Образ не найден
- Убедитесь, что образ загружен в Docker Hub
- Проверьте правильность имени репозитория и тега
- Убедитесь, что Docker Hub credentials настроены в App Platform

### Проблема: Сервис не запускается
- Проверьте переменные окружения
- Проверьте логи компонента через Dashboard
- Убедитесь, что порт 80 открыт и используется

