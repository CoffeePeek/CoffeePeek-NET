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

echo "==> Building and restarting application services..."
"${COMPOSE[@]}" up -d --build account shops moderation media gateway caddy

echo "==> Health check..."
sleep 5
curl -sf "http://localhost:${GATEWAY_PORT:-8080}/health" >/dev/null
echo "OK — gateway healthy"

"${COMPOSE[@]}" ps account shops moderation media gateway caddy
