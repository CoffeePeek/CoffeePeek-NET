#!/usr/bin/env bash
# Pull latest app images and restart services (used by CI and manual updates).
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "${DEPLOY_DIR}"

set -a
# shellcheck disable=SC1091
source .env
set +a

COMPOSE=(docker compose)
if [[ "${USE_HTTPS:-1}" == "1" ]]; then
  COMPOSE+=(--profile https)
fi

if [[ -n "${DOCKER_HUB_USERNAME:-}" && -n "${DOCKER_HUB_PASSWORD:-}" ]]; then
  echo "==> Logging in to Docker Hub..."
  echo "${DOCKER_HUB_PASSWORD}" | docker login -u "${DOCKER_HUB_USERNAME}" --password-stdin
fi

wait_for_backend() {
  local service="$1"
  local path="${2:-/health}"
  local attempts="${3:-40}"

  echo "==> Waiting for ${service} (${path})..."
  for ((i = 1; i <= attempts; i++)); do
    if docker run --rm --network coffeepeek_default curlimages/curl:8.10.1 \
      -sf "http://${service}${path}" >/dev/null 2>&1; then
      echo "${service} is ready"
      return 0
    fi
    sleep 3
  done

  echo "${service} did not become ready in time"
  return 1
}

use_registry_images=false
if [[ -n "${DOCKER_HUB_USERNAME:-}" && -n "${DOCKER_HUB_PASSWORD:-}" ]]; then
  use_registry_images=true
fi

if [[ "${use_registry_images}" == "true" ]]; then
  echo "==> Pulling backend images from Docker Hub..."
  "${COMPOSE[@]}" pull account shops moderation media
  echo "==> Starting backend services..."
  "${COMPOSE[@]}" up -d account shops moderation media
else
  echo "==> Building backend services (no registry credentials)..."
  "${COMPOSE[@]}" up -d --build account shops moderation media
fi

wait_for_backend shops /api/Catalogs/cities
wait_for_backend account /health

echo "==> Starting gateway and caddy..."
if [[ "${use_registry_images}" == "true" ]]; then
  "${COMPOSE[@]}" pull gateway
  "${COMPOSE[@]}" up -d gateway caddy
else
  "${COMPOSE[@]}" up -d --build gateway caddy
fi

echo "==> Health check..."
sleep 3
curl -sf "http://localhost:${GATEWAY_PORT:-8080}/api/Catalogs/cities" >/dev/null
echo "OK — gateway can reach shops"

"${COMPOSE[@]}" ps account shops moderation media gateway caddy
