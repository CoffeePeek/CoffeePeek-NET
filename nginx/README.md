# Nginx Gateway Configuration

Этот nginx gateway использует переменные окружения для динамической настройки маршрутизации к микросервисам.

## Переменные окружения

Для работы nginx gateway необходимо задать следующие переменные окружения в Railway для сервиса `nginx-gateway`:

### Обязательные переменные:

- `AUTH_SERVICE_NAME` - имя сервиса аутентификации (по умолчанию: `auth-service`)
- `USER_SERVICE_NAME` - имя сервиса пользователей (по умолчанию: `user-service`)
- `SHOPS_SERVICE_NAME` - имя сервиса магазинов (по умолчанию: `shops-service`)
- `MODERATION_SERVICE_NAME` - имя сервиса модерации (по умолчанию: `moderation-service`)
- `PHOTO_API_SERVICE_NAME` - имя сервиса фото API (по умолчанию: `photo-api`)
- `WEB_SERVICE_NAME` - имя веб-приложения (по умолчанию: `web`)

### Опциональные переменные:

- `CONTENT_SERVICE_NAME` - имя сервиса контента (для будущего использования)
- `AUDIT_SERVICE_NAME` - имя сервиса аудита (для будущего использования)

## Настройка в Railway

1. Откройте сервис `nginx-gateway` в Railway Dashboard
2. Перейдите в раздел "Variables"
3. Добавьте переменные окружения с именами сервисов из `railway.json`

### Пример значений:

```
AUTH_SERVICE_NAME=auth-service
USER_SERVICE_NAME=user-service
SHOPS_SERVICE_NAME=shops-service
MODERATION_SERVICE_NAME=moderation-service
PHOTO_API_SERVICE_NAME=photo-api
WEB_SERVICE_NAME=web
```

## Как это работает

1. При запуске контейнера `docker-entrypoint.sh` обрабатывает template файл `gateway.conf.template`
2. `envsubst` заменяет все переменные `${VARIABLE_NAME}` на значения из окружения
3. Результирующий файл `gateway.conf` сохраняется в `/etc/nginx/conf.d/`
4. Nginx запускается с обновленной конфигурацией

## Формат имен сервисов

Railway автоматически создает внутренние DNS имена в формате:
```
{service-name}.railway.internal
```

Где `{service-name}` - это значение из поля `name` в `railway.json`.

## Проверка конфигурации

После деплоя проверьте логи nginx:
```bash
railway logs --service nginx-gateway
```

Убедитесь, что нет ошибок подключения к сервисам.

