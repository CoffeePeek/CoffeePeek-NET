#!/bin/sh
set -e

# Устанавливаем значения по умолчанию для Auth Service
export AUTH_HOST=${AUTH_HOST:-auth-service.railway.internal}
export AUTH_PORT=${AUTH_PORT:-80}

# User Service
export USER_HOST=${USER_HOST:-user-service.railway.internal}
export USER_PORT=${USER_PORT:-80}

# Shops Service
export SHOPS_HOST=${SHOPS_HOST:-shops-service.railway.internal}
export SHOPS_PORT=${SHOPS_PORT:-80}

# Moderation Service
export MODERATION_HOST=${MODERATION_HOST:-moderation-service.railway.internal}
export MODERATION_PORT=${MODERATION_PORT:-80}

# Content Service
export CONTENT_HOST=${CONTENT_HOST:-content-service.railway.internal}
export CONTENT_PORT=${CONTENT_PORT:-80}

# Audit Service
export AUDIT_HOST=${AUDIT_HOST:-audit-service.railway.internal}
export AUDIT_PORT=${AUDIT_PORT:-80}

# Photo API
export PHOTO_HOST=${PHOTO_HOST:-photo-api.railway.internal}
export PHOTO_PORT=${PHOTO_PORT:-80}

# Web Application
export WEB_HOST=${WEB_HOST:-web.railway.internal}
export WEB_PORT=${WEB_PORT:-80}

# Replace environment variables in nginx config
envsubst '${AUTH_HOST} ${AUTH_PORT} ${USER_HOST} ${USER_PORT} ${SHOPS_HOST} ${SHOPS_PORT} ${MODERATION_HOST} ${MODERATION_PORT} ${CONTENT_HOST} ${CONTENT_PORT} ${AUDIT_HOST} ${AUDIT_PORT} ${PHOTO_HOST} ${PHOTO_PORT} ${WEB_HOST} ${WEB_PORT}' < /etc/nginx/conf.d/gateway.conf.template > /etc/nginx/conf.d/default.conf

# Start nginx
exec "$@"

