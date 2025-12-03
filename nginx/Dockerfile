FROM nginx:alpine

# Копируем конфигурацию
COPY conf.d/ /etc/nginx/conf.d/

# Открываем порт
EXPOSE 80

# Запускаем nginx
CMD ["nginx", "-g", "daemon off;"]

