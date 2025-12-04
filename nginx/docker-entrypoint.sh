#!/bin/sh
set -e

# Устанавливаем значения по умолчанию, если переменные не заданы
export AUTH_SERVICE_NAME=${AUTH_SERVICE_NAME:-auth-service}
export USER_SERVICE_NAME=${USER_SERVICE_NAME:-user-service}
export SHOPS_SERVICE_NAME=${SHOPS_SERVICE_NAME:-shops-service}
export MODERATION_SERVICE_NAME=${MODERATION_SERVICE_NAME:-moderation-service}
export PHOTO_API_SERVICE_NAME=${PHOTO_API_SERVICE_NAME:-photo-api}
export WEB_SERVICE_NAME=${WEB_SERVICE_NAME:-web}
export CONTENT_SERVICE_NAME=${CONTENT_SERVICE_NAME:-content-service}
export AUDIT_SERVICE_NAME=${AUDIT_SERVICE_NAME:-audit-service}

# Заменяем переменные окружения в template файле
envsubst < /etc/nginx/templates/gateway.conf.template > /etc/nginx/conf.d/gateway.conf

# Запускаем nginx
exec nginx -g 'daemon off;'

