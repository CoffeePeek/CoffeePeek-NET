# Настройка Nginx Gateway в Railway

## Быстрая настройка

После деплоя сервиса `nginx-gateway` в Railway, добавьте следующие переменные окружения в настройках сервиса:

### Переменные окружения для nginx-gateway:

```
AUTH_SERVICE_NAME=auth-service
USER_SERVICE_NAME=user-service
SHOPS_SERVICE_NAME=shops-service
MODERATION_SERVICE_NAME=moderation-service
PHOTO_API_SERVICE_NAME=photo-api
WEB_SERVICE_NAME=web
```

### Опциональные (для будущих сервисов):

```
CONTENT_SERVICE_NAME=content-service
AUDIT_SERVICE_NAME=audit-service
```

## Как добавить переменные в Railway:

1. Откройте проект в Railway Dashboard
2. Выберите сервис `nginx-gateway`
3. Перейдите в раздел **Variables**
4. Нажмите **+ New Variable** для каждой переменной
5. Введите имя переменной и значение (например, `AUTH_SERVICE_NAME` = `auth-service`)
6. Сохраните изменения

## Важно:

- Имена сервисов должны точно соответствовать значениям `name` в файле `railway.json`
- Если переменная не задана, будет использовано значение по умолчанию из `docker-entrypoint.sh`
- После изменения переменных окружения Railway автоматически перезапустит сервис

## Проверка работы:

После настройки проверьте логи:
```bash
railway logs --service nginx-gateway
```

Убедитесь, что нет ошибок и nginx успешно запустился.

## Тестирование:

Проверьте доступность сервисов через gateway:
- `GET /api/auth/health` - Auth Service
- `GET /api/user/...` - User Service  
- `GET /api/shops/...` - Shops Service
- `GET /api/moderation/...` - Moderation Service

