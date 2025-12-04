FROM nginx:alpine

# Устанавливаем gettext для envsubst
RUN apk add --no-cache gettext

# Удаляем дефолтный конфиг
RUN rm -f /etc/nginx/conf.d/default.conf

# Создаем директорию для templates
RUN mkdir -p /etc/nginx/templates

# Копируем основной конфиг nginx
COPY nginx.conf /etc/nginx/nginx.conf

# Копируем template файл
COPY conf.d/gateway.conf.template /etc/nginx/templates/gateway.conf.template

# Копируем entrypoint скрипт
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/docker-entrypoint.sh"]

