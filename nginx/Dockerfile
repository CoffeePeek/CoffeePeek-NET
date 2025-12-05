FROM nginx:stable-alpine3.20

# Install envsubst
RUN apk add --no-cache gettext

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Copy nginx main config
COPY nginx.conf /etc/nginx/nginx.conf

# Copy nginx configuration template
COPY conf.d/gateway.conf.template /etc/nginx/conf.d/

# Copy entrypoint script
COPY docker-entrypoint.sh /
RUN chmod +x /docker-entrypoint.sh

# Set entrypoint
ENTRYPOINT ["/docker-entrypoint.sh"]

CMD ["nginx", "-g", "daemon off;"]

